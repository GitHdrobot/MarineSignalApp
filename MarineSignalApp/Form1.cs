using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Reflection.Emit;

using System.Runtime.InteropServices;//DllImport

using System.ServiceModel.Channels;//BufferManager

using ipp;//ipp 
using System.IO;

using JerryMouse;
using System.Collections.Concurrent;
using System.Threading;
using DevExpress.Xpo.Logger;
// Import log4net classes.
using log4net;
using log4net.Config;
using log4net.Core;

namespace MarineSignalApp
{

    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public const int MAX_POOL_SIZE = 209715200;//200M
        public const int MAX_IND_SIZE = 2097152;//2M
        public const int MAX_NUM_BUFF = MAX_POOL_SIZE / MAX_IND_SIZE;
        public const int NUM_OF_BUFF = 100;

        public MarineHFrame hframe;
        protected FileStream sigDatafs;
        protected BinaryReader sigDatabr;

        protected bool bReadDone, bRunning, bQuit;

        public ConcurrentQueue<byte[]> mdataqueue;
        AutoResetEvent dataQueueevent;
        //相干码矩阵
        //Ipp32fc[] mArrayCorrCode;
        IntPtr hglobalCorre;
        // Define a static logger variable so that it references the
        // Logger instance named "Form1".
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(Form1));

        //测试运行时间
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern bool QueryPerformanceCounter(ref long count);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern bool QueryPerformanceFrequency(ref long count);

        //Windows 组件 初始化
        public Form1()
        {
            InitializeComponent();
            InitialConfig();
        }

        private void checkedListBoxControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                fileDlg = new OpenFileDialog();
                fileDlg.Filter = "data Documents(*.bin;*.dat)|*.bin;*.dat|All Files(*.*)|*.*";
                fileDlg.ShowReadOnly = true;
                DialogResult r = fileDlg.ShowDialog();
                if (r == DialogResult.OK)//打开成功
                {
                    this.textEdit3.Text = fileDlg.FileName;
                }
            }
            catch (Exception)
            {

                Console.Write("open error");
            }

        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            try
            {
                this.simpleButton2.Enabled = false;
                this.simpleButton3.Enabled = true;
                bRunning = true;

                fileDlg.FileName = this.textEdit3.Text;
                sigDatafs = new FileStream(fileDlg.FileName, FileMode.Open);
                sigDatabr = new BinaryReader(sigDatafs);

                if (BgFetchDataWorker.IsBusy != true)
                {
                    // 启动异步操作
                    BgFetchDataWorker.RunWorkerAsync(this);
                    bgSyncHFrameWorker.RunWorkerAsync(this);
                }

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("file not found");
            }
            catch (InvalidAsynchronousStateException)
            {
                Console.WriteLine("thread start failed");
            }
            catch (IOException)
            {
                Console.WriteLine("file is opened");

            }

        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (BgFetchDataWorker.WorkerSupportsCancellation == true)
            {
                // 取消异步操作
                BgFetchDataWorker.CancelAsync();
            }
            this.simpleButton3.Enabled = false;
            this.simpleButton2.Enabled = true;
            bRunning = false;
        }

        //进行内存申请 和 初始化
        public void RtMemInitial()
        {
            try
            {

            }
            catch (InsufficientMemoryException e)
            {
                Console.Write(e.StackTrace);
            }
        }

        public void RtRetureBuffer()
        {


        }

        //进行必要的初始化工作
        public void InitialConfig()
        {
            //加载log配置文件
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var logger = new log4net.Appender.MemoryAppender();

            log4net.Config.BasicConfigurator.Configure(logger);


            RtMemInitial();
            //创建一个HFrame实例
            hframe = new MarineHFrame();
            //buff
            mdataqueue = new ConcurrentQueue<byte[]>();
            //mArrayCorrCode = new Ipp32fc[MarineHFrame.ccorreCodeNum];
            hglobalCorre = Marshal.AllocHGlobal(MarineHFrame.cbytesOfCorreCode);
            byte[] tb = new byte[MarineHFrame.cbytesOfCorreCode];

            ReadCorreCode(ref tb);
            Marshal.Copy(tb, 0, hglobalCorre, MarineHFrame.cbytesOfCorreCode);
            bReadDone = false;
            bQuit = false;
            this.simpleButton3.Enabled = false;
            this.simpleButton2.Enabled = true;
            this.textEdit1.Text = hframe.bytesNumOneCircle + "";
            this.textEdit2.Text = 0.ToString();
            this.textEdit3.Text = RtFilePath.signalFilePath;

            BgFetchDataWorker.WorkerSupportsCancellation = true;
            BgFetchDataWorker.DoWork += BgFetchDataWorker_DoWork;
            BgFetchDataWorker.WorkerReportsProgress = true;
            BgFetchDataWorker.ProgressChanged += BgFetchDataWorker_ProgressChanged;
            BgFetchDataWorker.RunWorkerCompleted += BgFetchDataWorker_RunWorkerCompleted;

            bgSyncHFrameWorker.WorkerSupportsCancellation = true;
            bgSyncHFrameWorker.DoWork += bgSyncHFrameWorker_DoWork;
            bgSyncHFrameWorker.WorkerReportsProgress = true;
            bgSyncHFrameWorker.ProgressChanged += bgSyncHFrameWorker_ProgressChanged;
            bgSyncHFrameWorker.RunWorkerCompleted += bgSyncHFrameWorker_RunWorkerCompleted;
            dataQueueevent = new AutoResetEvent(false);
        }

        //读取相干码文件
        public bool ReadCorreCode(ref byte[] buffer)
        {
            try
            {
                FileStream fscorre = new FileStream(RtFilePath.correcodeFilePath, FileMode.Open);
                BinaryReader brcorre = new BinaryReader(fscorre);
                int len = brcorre.Read(buffer, 0, MarineHFrame.cbytesOfCorreCode);
                if (len != MarineHFrame.cbytesOfCorreCode)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
            return true;

        }

        //数据读取主任务实现
        private unsafe void BgFetchDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (sigDatabr == null)
            {
                return;
            }
            //定义测试时间所需变量
            long count = 0;
            long count1 = 0;
            long freq = 0;
            double result = 0;

            try
            {
                byte[] bfwbuff;
                while (bRunning)
                {
                    //运行时间
                    QueryPerformanceFrequency(ref freq);
                    QueryPerformanceCounter(ref count);

                    if (bQuit)
                    {
                        return;
                    }
                    //if (bReadDone)
                    //{
                    //    Console.WriteLine("数据读取完成");
                    //    System.Threading.Thread.Sleep(3000);
                    //    continue;
                    //}
                    if (mdataqueue.Count > NUM_OF_BUFF)
                    {
                        Console.WriteLine("数据缓冲区已满 等待处理");
                        System.Threading.Thread.Sleep(1500);
                        continue;
                    }
                    if (sigDatabr.BaseStream.Position < sigDatabr.BaseStream.Length)
                    {

                        //readLen = sigDatabr.Read(bfwbuff, 0, hframe.bytesNumOneCircle);
                        bfwbuff = sigDatabr.ReadBytes(hframe.bytesNumOneCircle);
                        if (bfwbuff != null && bfwbuff.Length == hframe.bytesNumOneCircle)
                        {
                            mdataqueue.Enqueue(bfwbuff);
                        }
                        else {
                            continue;
                        }

                    }
                    else {
                        bReadDone = true;
                        Console.WriteLine("数据读取完成");
                        break;
                    }

                    QueryPerformanceCounter(ref count1);
                    count = count1 - count;
                    result = (double)(count) / (double)freq;
                    Console.WriteLine("读取102帧时间: {0} 秒", result);
                }
                //System.Threading.Thread.Sleep(1000);
                //continue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("处理数据线程异常" + Thread.CurrentThread.Name);
            }
            finally
            {
            }

        }
        //更新进度
        private void BgFetchDataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        //处理后台操作结果
        private void BgFetchDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                Console.WriteLine("退出数据读取线程");
            }
            else if (e.Error != null)
            {

            }
            else
            {

            }
        }
        //数据处理任务

        private unsafe void bgSyncHFrameWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            int sumsz = sizeof(float) * hframe.slidelen;

            IntPtr hAbsCorSumSt = (IntPtr)null;
            IntPtr hAbsCorSumSnd = (IntPtr)null;

            IntPtr hglobalDataSt = (IntPtr)null;
            IntPtr hglobalDataSnd = (IntPtr)null;

            IntPtr hAbsCorSumTrace = (IntPtr)null;
            //定义测试时间所需变量
            long count = 0;
            long count1 = 0;
            long freq = 0;
            double result = 0;

            try
            {

                //float[] absCorSumSt = new float[hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum];
                //float[] absCorSumSnd = new float[hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum];

                //Ipp32fc[] IpcDataSt = new Ipp32fc[hframe.sampleNumOneCircle];
                //Ipp32fc[] IpcDataSnd = new Ipp32fc[hframe.sampleNumOneCircle];
                //unmanaged buffer


                hAbsCorSumSt = Marshal.AllocHGlobal(sumsz);
                hAbsCorSumSnd = Marshal.AllocHGlobal(sumsz);

                hglobalDataSt = Marshal.AllocHGlobal(hframe.bytesNumOneCircle);
                hglobalDataSnd = Marshal.AllocHGlobal(hframe.bytesNumOneCircle);

                float fmaxst = 0.0f, fmaxsnd = 0.0f;
                int iIndxst = -1, iIndxsnd = -1, iCorrectIdx = -1;
                //跟踪时 取数据的长度
                int hlen = hframe.HFrameTraceRghtOffset + hframe.HFrameTraceLftOffset;
                //跟踪时 相关求和的长度
                int htraceSumLen = hlen - MarineHFrame.ccorreCodeNum;
                hAbsCorSumTrace = Marshal.AllocHGlobal(htraceSumLen);
    

                int indxTrace = -1;
                float maxTrace = -1.0f;

                while (true)
                {
                    if (mdataqueue.IsEmpty)
                    {
                        Console.WriteLine("数据为空 等待中");
                        Thread.Sleep(1000);
                        continue;
                    }
                    while (!mdataqueue.IsEmpty)
                    {
                        QueryPerformanceFrequency(ref freq);
                        QueryPerformanceCounter(ref count);

                        byte[] st = null, snd = null;

                        if (bQuit)
                        {
                            break;
                        }

                        if (!hframe.HFrameSyncState && mdataqueue.Count < rtdef.TWO)
                        {
                            Console.WriteLine("数据已处理完成 等待中");
                            System.Threading.Thread.Sleep(10);
                            continue;
                        }
                        //GCHandle pinnedObj = GCHandle.Alloc(anObj, GCHandleType.Pinned);
                        if (!mdataqueue.TryDequeue(out st) || st == null)
                        {
                            Console.WriteLine("取第一个数据失败");
                            continue;
                        }
                        if (st.Length != hframe.bytesNumOneCircle)
                        {
                            Console.WriteLine("Impossible!!!");
                            continue;
                        }
                        Marshal.Copy(st, 0, hglobalDataSt, hframe.bytesNumOneCircle);

                        //同步操作 寻找H帧的位置
                        #region 
                        if (hframe.HFrameSyncState == false)
                        {
                            if (!mdataqueue.TryDequeue(out snd) || snd == null)
                            {
                                Console.WriteLine("取第二个数据失败");
                                continue;
                            }
                            if (snd.Length != hframe.bytesNumOneCircle)
                            {
                                Console.WriteLine("Impossible!!!");
                                continue;
                            }
                            Marshal.Copy(snd, 0, hglobalDataSnd, hframe.bytesNumOneCircle);
                            GetHFrameLocation((Ipp32fc*)hglobalDataSt, hframe.sampleNumOneCircle, (float*)hAbsCorSumSt, hframe.slidelen, ref fmaxst, ref iIndxst);
                            GetHFrameLocation((Ipp32fc*)hglobalDataSnd, hframe.sampleNumOneCircle, (float*)hAbsCorSumSnd, hframe.slidelen, ref fmaxsnd, ref iIndxsnd);
                            if (iIndxsnd < 0 || iIndxst < 0)
                            {
                                //未找到H帧
                                Console.WriteLine("同步中，两个循环中都未寻找到");
                                continue;
                            }
                            if (Math.Abs(iIndxsnd - iIndxst) < MarineHFrame.crange)
                            {
                                //即两个都正确
                                Console.WriteLine("同步中，有两个循环中寻找到");
                                iCorrectIdx = iIndxsnd;

                            }
                            else {
                                //即有一个正确
                                GetCorrectHIndx((float*)hAbsCorSumSt, hframe.slidelen, iIndxst, (float*)hAbsCorSumSnd, hframe.slidelen, iIndxsnd, &iCorrectIdx);
                                Console.WriteLine("同步中，有一个循环中寻找到");
                            }

                            hframe.HFrameSyncState = true;
                            hframe.HFrameLocation = iCorrectIdx;
                        }
                        #endregion
                        //跟踪H帧位置
                        #region 
                        else {
                            //H帧 寻找的起始位置
                            int hstartIndx = hframe.HFrameLocation - hframe.HFrameTraceLftOffset <= 0 ? 0 : hframe.HFrameLocation - hframe.HFrameTraceLftOffset;
                            if (hstartIndx + hlen >= hframe.sampleNumOneCircle)
                            {
                                Console.WriteLine("跟踪时，已越界");
                                continue;//越界
                            }
                            Ipp32fc* hgstart = (Ipp32fc*)hglobalDataSt + hstartIndx;
                            GetHFrameLocation(hgstart, hlen, (float*)hAbsCorSumTrace, htraceSumLen, ref maxTrace, ref indxTrace);
                            //跟踪失败 已失步
                            if (maxTrace < 0 || indxTrace < 0)
                            {
                                Console.WriteLine("跟踪时，未寻找到");
                                hframe.HFrameSyncState = false;
                            }
                            else {
                                hframe.HFrameSyncState = true;
                                hframe.HFrameLocation = hstartIndx + indxTrace;
                                Console.WriteLine("跟踪时，成功");
                            }
                        }
                        #endregion
                        Console.WriteLine("HFrameLocation:" + hframe.HFrameLocation);
                    }
                    if (bQuit)
                    {
                        break;
                    }


                    QueryPerformanceCounter(ref count1);
                    count = count1 - count;
                    result = (double)(count) / (double)freq;
                    Console.WriteLine("H帧寻找时间: {0} 秒", result);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("读取数据线程异常" + Thread.CurrentThread.Name);
            }

            finally
            {
                if (null != hAbsCorSumSt)
                {
                    Marshal.FreeHGlobal(hAbsCorSumSt);
                }
                if (null != hAbsCorSumSt)
                {
                    Marshal.FreeHGlobal(hAbsCorSumSnd);
                }

                if (null != hAbsCorSumSt)
                {
                    Marshal.FreeHGlobal(hglobalDataSt);
                }
                if (null != hAbsCorSumSt)
                {
                    Marshal.FreeHGlobal(hglobalDataSnd);
                }
                if (null != hAbsCorSumTrace)
                {
                    Marshal.FreeHGlobal(hAbsCorSumTrace);
                }


            }

        }
        private void bgSyncHFrameWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) { }
        private void bgSyncHFrameWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                Console.WriteLine("退出H帧同步线程");
            }
            else if (e.Error != null)
            {

            }
            else
            {

            }
        }


        ////同步操作
        //unsafe void SyncHFrame(int syntimes,ref int hframloc) {


        //}

        // 查找H帧的位置 并返回
        unsafe void GetHFrameLocation(Ipp32fc* pIpcData, int ipcDataLen, float* pAbsCorreSum, int sumLen, ref float pfmax, ref int piIndx)
        {
            float fmax = -1.0f;
            int iIndx = -1;
            Ipp32fc* pnIpcData = null;
            try
            {
                //pnIpcData 存放做完相干处理后的数据
                pnIpcData = sp.ippsMalloc_32fc(ipcDataLen);
                //相干处理
                CorrelatorProcess(pIpcData, ipcDataLen, pnIpcData, ipcDataLen);
                //相干码滑动 求得一个和值
                SlideMatchHFrame(pnIpcData, ipcDataLen, (Ipp32fc*)hglobalCorre, MarineHFrame.ccorreCodeNum, pAbsCorreSum, sumLen);
                //在和值中 求出最大值
                sp.ippsMaxIndx_32f(pAbsCorreSum, sumLen, &fmax, &iIndx);
                if (iIndx + MarineHFrame.ccmpOffset > sumLen || iIndx - MarineHFrame.ccmpOffset < 0)
                {
                    Console.WriteLine("Possible!!!");
                }
                //判断最大值是否满足H帧 规律性条件
                if ((fmax / pAbsCorreSum[iIndx + MarineHFrame.ccmpOffset] > MarineHFrame.cflthreshold) && (fmax / pAbsCorreSum[iIndx - MarineHFrame.ccmpOffset] > MarineHFrame.cflthreshold))
                {
                    pfmax = fmax;
                    piIndx = iIndx;
                }
            }

            catch (DllNotFoundException e)
            {
                Console.WriteLine(e.StackTrace);

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if (null != pnIpcData)
                {
                    sp.ippsFree(pnIpcData);
                }

            }
        }

        // 做相干处理
        unsafe int CorrelatorProcess(Ipp32fc* pSrc, int srclen, Ipp32fc* pDst, int dstlen)
        {
            Ipp32fc* pSrcConj = null;
            try
            {
                //Ipp32fc* ippsMalloc_32fc(int len);
                pSrcConj = sp.ippsMalloc_32fc(srclen);
                sp.ippsZero_32fc(pSrcConj, srclen);
                //求pSrc的共轭
                //IppStatus ippsConj_32fc(const Ipp32fc* pSrc, Ipp32fc* pDst, int len);
                sp.ippsConj_32fc(pSrc, pSrcConj, srclen);
                //IppStatus ippsMul_32fc(const Ipp32fc* pSrc1, const Ipp32fc* pSrc2, Ipp32fc*pDst, int len);
                sp.ippsMul_32fc(pSrc + 1, pSrcConj, pDst, srclen - 1);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return -1;
            }
            finally
            {
                if (null != pSrcConj)
                {
                    sp.ippsFree(pSrcConj);
                }

            }
            return 0;
        }

        //滑动匹配H帧
        unsafe int SlideMatchHFrame(Ipp32fc* pSrc, int slen, Ipp32fc* pCorrCode, int corrLen, float* pAbsSum, int absSumLen)
        {
            Ipp32fc* ptmpSlide = null;
            Ipp32fc* pCorreSum = null;
            Ipp32fc* pDst = null;
            try
            {
                //Ipp32fc* ippsMalloc_32fc(int len);
                ptmpSlide = sp.ippsMalloc_32fc(corrLen);
                //Ipp32f *pAbsCorreSum = ippsMalloc_32f(slen - corrLen);
                pCorreSum = sp.ippsMalloc_32fc(absSumLen);
                pDst = sp.ippsMalloc_32fc(corrLen);
                //IppStatus ippsZero_32fc(Ipp32fc* pDst, int len);
                sp.ippsZero_32fc(ptmpSlide, corrLen);
                sp.ippsZero_32fc(pCorreSum, absSumLen);
                sp.ippsZero_32f(pAbsSum, absSumLen);
                sp.ippsZero_32fc(pDst, corrLen);
                for (int i = 0; i < slen - corrLen; i++)
                {
                    //memcpy(ptmpSlide,pSrc + i,corrLen * sizeof(float));
                    //IppStatus ippsCopy_32fc(const Ipp32fc* pSrc, Ipp32fc* pDst, int len);
                    sp.ippsCopy_32fc(pSrc + i, ptmpSlide, corrLen);
                    sp.ippsMul_32fc(ptmpSlide, pCorrCode, pDst, corrLen);
                    sp.ippsSum_32fc(pDst, corrLen, pCorreSum + i, IppHintAlgorithm.ippAlgHintNone);
                }
                //IppStatus ippsMagnitude_32fc(const Ipp32fc* pSrc, Ipp32f* pDst, int len);
                sp.ippsMagnitude_32fc(pCorreSum, pAbsSum, absSumLen);


            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return -1;
            }
            finally
            {
                if (null != ptmpSlide)
                {
                    sp.ippsFree(ptmpSlide);
                }
                if (null != pCorreSum)
                {
                    sp.ippsFree(pCorreSum);
                }
                if (null != pDst)
                {
                    sp.ippsFree(pDst);
                }
            }
            return 0;
        }

        unsafe private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            BgFetchDataWorker.CancelAsync();
            bgSyncHFrameWorker.CancelAsync();
            Marshal.FreeHGlobal(hglobalCorre);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 通过对两个循环帧的比对 找出正确的H帧位置
        unsafe int GetCorrectHIndx(float* pAbsSumSt, int lenSt, int indxSt, float* pAbsSumSnd, int lenSnd, int indxSnd, int* pCorrectIndx)
        {
            try
            {
                bool isSatisfyHCon = false, search_result = false;
                float fmaxCorrect;
                int iCorrectIdx;


                if (indxSnd - 2 * MarineHFrame.ccmpOffset < 1 || indxSnd - 2 * MarineHFrame.ccmpOffset >= lenSnd)
                {
                    Console.WriteLine("Possible!!!");
                }
                /********************R****************/
                /********************R********W*******/
                //h未寻找到 假设第一个寻找正确 第二个错误
                //第二个往前跳22帧 此处简单处理 从开始处寻找
                sp.ippsMaxIndx_32f(pAbsSumSnd, indxSnd - 2 * MarineHFrame.ccmpOffset, &fmaxCorrect, &iCorrectIdx);
                //判断最大值是否满足H帧 规律性条件
                PeakVerify(pAbsSumSnd, lenSnd, fmaxCorrect, iCorrectIdx, &isSatisfyHCon);
                //往前寻找结果
                if (isSatisfyHCon && Math.Abs(iCorrectIdx - indxSt) < MarineHFrame.crange)
                {
                    //寻找正确
                    search_result = true;
                    *pCorrectIndx = iCorrectIdx;
                }
                else {
                    /********************R****************/
                    /*************W******R****************/
                    //往后寻找 简单处理 截掉错误数据 
                    int startidx = indxSnd + 2 * MarineHFrame.ccmpOffset;
                    //去掉错误数据后的长度 
                    int sumSndllen = hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum - indxSnd - 2 * MarineHFrame.ccmpOffset;
                    if (startidx + sumSndllen >= lenSnd)
                    {
                        Console.WriteLine("Error");
                    }
                    sp.ippsMaxIndx_32f(pAbsSumSnd + startidx, sumSndllen, &fmaxCorrect, &iCorrectIdx);
                    //判断最大值是否满足H帧 规律性条件
                    PeakVerify(pAbsSumSnd, lenSnd, fmaxCorrect, iCorrectIdx, &isSatisfyHCon);
                    if (isSatisfyHCon && Math.Abs(iCorrectIdx - indxSt) < MarineHFrame.crange)
                    {
                        //寻找正确
                        search_result = true;
                        *pCorrectIndx = iCorrectIdx;
                    }
                }
                if (!search_result)// 假设第二个寻找正确 第一个错误
                {
                    /********************R******W*********/
                    /********************R****************/
                    //第一个往前跳22帧 此处简单处理 从开始处寻找
                    sp.ippsMaxIndx_32f(pAbsSumSt, indxSt - 2 * MarineHFrame.ccmpOffset, &fmaxCorrect, &iCorrectIdx);
                    //判断最大值是否满足H帧 规律性条件
                    PeakVerify(pAbsSumSt, lenSt, fmaxCorrect, iCorrectIdx, &isSatisfyHCon);
                    if (isSatisfyHCon && Math.Abs(iCorrectIdx - indxSnd) < MarineHFrame.crange)
                    {
                        //寻找正确
                        search_result = true;
                        *pCorrectIndx = iCorrectIdx;
                    }
                    else {
                        /************W********R***************/
                        /********************R****************/
                        //第一个往前跳22帧 此处简单处理 从开始处寻找
                        int startidx = indxSt + 2 * MarineHFrame.ccmpOffset;
                        int sumStllen = hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum - indxSt - 2 * MarineHFrame.ccmpOffset;
                        sp.ippsMaxIndx_32f(pAbsSumSt + startidx, sumStllen, &fmaxCorrect, &iCorrectIdx);
                        //判断最大值是否满足H帧 规律性条件
                        PeakVerify(pAbsSumSt, lenSt, fmaxCorrect, iCorrectIdx, &isSatisfyHCon);
                        if (isSatisfyHCon && Math.Abs(iCorrectIdx - indxSnd) < MarineHFrame.crange)
                        {
                            //寻找正确
                            search_result = true;
                            *pCorrectIndx = iCorrectIdx;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return -1;
            }
            return 0;
        }

        // 对波峰的判断
        unsafe int PeakVerify(float* pAbsSum, int sumlen, float fmax, int peakIndx, bool* presult)
        {
            try
            {
                int leftIndx = peakIndx - MarineHFrame.ccmpOffset <= 0 ? 0 : peakIndx - MarineHFrame.ccmpOffset;
                int rightIndx = peakIndx + MarineHFrame.ccmpOffset >= sumlen ? sumlen : peakIndx + MarineHFrame.ccmpOffset;
                //判断最大值是否满足H帧 规律性条件
                if ((fmax / pAbsSum[rightIndx] > MarineHFrame.cflthreshold) && (fmax / pAbsSum[leftIndx] > MarineHFrame.cflthreshold))
                {
                    *presult = true;
                    return 0;
                }
                *presult = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return -1;
            }
            return 0;
        }

    }
}
