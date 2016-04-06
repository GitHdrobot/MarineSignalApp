#region Using Directive
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
using log4net.Appender;
#endregion

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
        //相干码矩阵
        //Ipp32fc[] mArrayCorrCode;
        IntPtr hglobalCorre;
        //日志相关定义
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool logWatching = true;
        private log4net.Appender.MemoryAppender logger;
        private Thread logWatcher;

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
                log.Error("数据文件打开失败");
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
            catch (InvalidAsynchronousStateException)
            {
                log.Error("线程启动失败");
            }
            catch (IOException)
            {
                log.Error("文件操作异常");
            }

        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (BgFetchDataWorker.IsBusy)
            {
                // 取消异步操作
                BgFetchDataWorker.CancelAsync();
            }
            if (bgSyncHFrameWorker.IsBusy)
            {
                // 取消异步操作
                bgSyncHFrameWorker.CancelAsync();
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
            }
        }
        //进行必要的初始化工作
        public void InitialConfig()
        {
            //配置log appender
            this.Closing += new CancelEventHandler(Log_Closing);
            logger = new log4net.Appender.MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(logger);
            logWatcher = new Thread(new ThreadStart(LogWatcher));
            logWatcher.Start();

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

        }
        #region 日志相关处理函数
        void Log_Closing(object sender, CancelEventArgs e)
        {
            logWatching = false;
            logWatcher.Join();

        }
        delegate void AppendLogToCtrl(LoggingEvent logevent);
        void AppendLog(LoggingEvent logevent)
        {
            if (logContainer.InvokeRequired)
            {
                AppendLogToCtrl add = new AppendLogToCtrl(AppendLog);
                logContainer.Invoke(add, new object[] { logevent });
            }
            else
            {
                string _log = "[" + logevent.Level + "]  " + logevent.TimeStamp.ToLocalTime() + ">>  " + logevent.RenderedMessage;
                logContainer.Items.Add(_log);
            }
        }

        private void LogWatcher()
        {
            while (logWatching)
            {
                LoggingEvent[] events = logger.GetEvents();
                if (events != null && events.Length > 0)
                {
                    logger.Clear();
                    foreach (LoggingEvent ev in events)
                    {
                        AppendLog(ev);
                    }
                }
                Thread.Sleep(500);
            }
        }
        #endregion


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
                log.Error("相干码文件读取错误");
                log.Error(e.StackTrace);
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

            //运行时间
            QueryPerformanceFrequency(ref freq);
            QueryPerformanceCounter(ref count);

            try
            {
                byte[] bfwbuff;
                while (!BgFetchDataWorker.CancellationPending)
                {

                    if (bQuit)
                    {
                        return;
                    }
                    if (mdataqueue.Count > NUM_OF_BUFF)
                    {
                        log.Warn("数据缓冲区已满，等待处理。。。");
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
                        log.Debug("数据读取完成。。。");
                        break;
                    }
                }
                QueryPerformanceCounter(ref count1);
                count = count1 - count;
                result = (double)(count) / (double)freq;
                log.Debug("读取102帧时间:" + result + "秒");
                //System.Threading.Thread.Sleep(1000);
                //continue;
            }
            catch (Exception ex)
            {
                log.Error("数据读取线程异常:" + Thread.CurrentThread.Name);
                log.Error(ex.StackTrace);
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
                log.Debug("数据读取完全");
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

            //GCHandle gchandlest, gchandlesnd;

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
                hAbsCorSumTrace = Marshal.AllocHGlobal(sizeof(float) * htraceSumLen);


                int indxTrace = -1;
                float maxTrace = -1.0f;

                while (!bgSyncHFrameWorker.CancellationPending)
                {
                    if (mdataqueue.IsEmpty)
                    {
                        log.Debug("数据为空，等待中...");
                        Thread.Sleep(1000);
                        continue;
                    }
                    while (!mdataqueue.IsEmpty)
                    {
                        QueryPerformanceFrequency(ref freq);
                        QueryPerformanceCounter(ref count);

                        if (bQuit)
                        {
                            break;
                        }

                        if (!hframe.HFrameSyncState && mdataqueue.Count < rtdef.TWO)
                        {
                            log.Debug("数据已处理完成，等待中...");
                            System.Threading.Thread.Sleep(10);
                            continue;
                        }
                        byte[] st = null, snd = null;

                        //GCHandle pinnedObj = GCHandle.Alloc(anObj, GCHandleType.Pinned);
                        if (!mdataqueue.TryDequeue(out st) || st == null)
                        {
                            log.Warn("取第一个数据失败");
                            continue;
                        }
                        //gchandlest = GCHandle.Alloc(st, GCHandleType.Pinned);

                        //if (st.Length != hframe.bytesNumOneCircle)
                        //{
                        //    log.Debug("取第一个数据失败");
                        //    continue;
                        //}
                        //GCHandle.ToIntPtr(gchandlest);
                        Marshal.Copy(st, 0, hglobalDataSt, hframe.bytesNumOneCircle);

                        //同步操作 寻找H帧的位置
                        #region 
                        if (hframe.HFrameSyncState == false)
                        {
                            if (!mdataqueue.TryDequeue(out snd) || snd == null)
                            {
                                log.Warn("取第二个数据失败");
                                continue;
                            }
                            //gchandlesnd = GCHandle.Alloc(snd, GCHandleType.Pinned);
                            //if (snd.Length != hframe.bytesNumOneCircle)
                            //{
                            //    Console.WriteLine("Impossible!!!");
                            //    continue;
                            //}
                            Marshal.Copy(snd, 0, hglobalDataSnd, hframe.bytesNumOneCircle);
                            GetHFrameLocation((Ipp32fc*)hglobalDataSt, hframe.sampleNumOneCircle, (float*)hAbsCorSumSt, hframe.slidelen, ref fmaxst, ref iIndxst);
                            GetHFrameLocation((Ipp32fc*)hglobalDataSnd, hframe.sampleNumOneCircle, (float*)hAbsCorSumSnd, hframe.slidelen, ref fmaxsnd, ref iIndxsnd);
                            if (iIndxsnd < 0 || iIndxst < 0)
                            {
                                //未找到H帧
                                log.Debug("同步中，两个循环中都未寻找到");
                                continue;
                            }
                            if (Math.Abs(iIndxsnd - iIndxst) < MarineHFrame.crange)
                            {
                                //即两个都正确
                                log.Debug("同步中，两个循环中都未寻找到");
                                iCorrectIdx = iIndxsnd;

                            }
                            else {
                                //即有一个正确
                                GetCorrectHIndx((float*)hAbsCorSumSt, hframe.slidelen, iIndxst, (float*)hAbsCorSumSnd, hframe.slidelen, iIndxsnd, &iCorrectIdx);
                                log.Debug("同步中，有一个循环中寻找到");
                            }

                            hframe.HFrameSyncState = true;
                            hframe.HFrameLocation = iCorrectIdx;
                            log.Debug("H帧同步成功，H帧位置：" + hframe.HFrameLocation);
                        }
                        #endregion
                        //跟踪H帧位置
                        #region 
                        else {
                            //H帧 寻找的起始位置
                            int hstartIndx = hframe.HFrameLocation - hframe.HFrameTraceLftOffset <= 0 ? 0 : hframe.HFrameLocation - hframe.HFrameTraceLftOffset;
                            if (hstartIndx + hlen >= hframe.sampleNumOneCircle)
                            {
                                log.Warn("跟踪时，已越界");
                                continue;//越界
                            }
                            Ipp32fc* hgstart = (Ipp32fc*)hglobalDataSt + hstartIndx;
                            GetHFrameLocation((Ipp32fc*)hgstart, hlen, (float*)hAbsCorSumSnd, htraceSumLen, ref maxTrace, ref indxTrace);

                            //跟踪失败 已失步
                            if (maxTrace < 0 || indxTrace < 0)
                            {
                                log.Debug("跟踪时，未寻找到");
                                hframe.HFrameSyncState = false;
                            }
                            else {
                                hframe.HFrameSyncState = true;
                                hframe.HFrameLocation = hstartIndx + indxTrace;
                                log.Debug("跟踪时，成功");
                                log.Debug("H帧位置:" + hframe.HFrameLocation);
                            }
                        }
                        #endregion

                    }
                    if (bQuit)
                    {
                        break;
                    }


                    QueryPerformanceCounter(ref count1);
                    count = count1 - count;
                    result = (double)(count) / (double)freq;
                    log.Debug("H帧寻找时间:" + result + " 秒");
                }

            }
            catch (Exception ex)
            {
                log.Warn("数据处理线程异常");
                log.Warn(ex.StackTrace);
            }

            finally
            {
                if (null != hAbsCorSumSt)
                {
                    Marshal.FreeHGlobal(hAbsCorSumSt);
                }
                if (null != hAbsCorSumSnd)
                {
                    Marshal.FreeHGlobal(hAbsCorSumSnd);
                }

                if (null != hglobalDataSt)
                {
                    Marshal.FreeHGlobal(hglobalDataSt);
                }
                if (null != hglobalDataSnd)
                {
                    Marshal.FreeHGlobal(hglobalDataSnd);
                }
                if (null != hAbsCorSumTrace)
                {
                    Marshal.FreeHGlobal(hAbsCorSumTrace);
                }
                if (null != hglobalCorre)
                {
                    Marshal.FreeHGlobal(hglobalCorre);
                }
            }

        }
        private void bgSyncHFrameWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) { }
        private void bgSyncHFrameWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                log.Debug("退出H帧同步线程");
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
                    log.Warn("H帧靠近两端");
                }
                //判断最大值是否满足H帧 规律性条件
                if ((fmax / pAbsCorreSum[iIndx + MarineHFrame.ccmpOffset] > MarineHFrame.cflthreshold) && (fmax / pAbsCorreSum[iIndx - MarineHFrame.ccmpOffset] > MarineHFrame.cflthreshold))
                {
                    pfmax = fmax;
                    piIndx = iIndx;
                }
            }

            catch (Exception e)
            {
                log.Warn(e.StackTrace);
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

                log.Error(e.StackTrace);
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
                //sp.ippsZero_32f(pAbsSum, absSumLen);
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
                log.Warn(e.StackTrace);
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

        private void groupControl1_DoubleClick(object sender, EventArgs e)
        {
            logContainer.Items.Clear();
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
                    log.Warn("H帧在靠近数据两端");
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
                        log.Error("计算错误");
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
                log.Error(e.StackTrace);
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
                log.Error(e.StackTrace);
                return -1;
            }
            return 0;
        }

    }
}
