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
                fileDlg.Filter = "data Documents(*.bin;*.dat)|(*.bin;*.dat)|All Files(*.*)|(*.*)";
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
        }
        //读取相干码文件
        public void ReadCorreCode(ref byte[] buffer)
        {
            try
            {
                FileStream fscorre = new FileStream(RtFilePath.correcodeFilePath, FileMode.Open);
                BinaryReader brcorre = new BinaryReader(fscorre);
                brcorre.Read(buffer, 0, MarineHFrame.cbytesOfCorreCode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }


        }

        //数据读取主任务实现
        private unsafe void BgFetchDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Form1 form = e.Argument as Form1;
            int readLen = 0;
            if (sigDatabr == null)
            {
                return;
            }
            try
            {
                byte[] bfwbuff = new byte[hframe.bytesNumOneCircle];
                while (true)
                {

                    while (bRunning && !bReadDone)
                    {

                        if (bQuit)
                        {
                            return;
                        }
                        if (mdataqueue.Count > NUM_OF_BUFF)
                        {
                            System.Threading.Thread.Sleep(400);
                            continue;
                        }
                        if (sigDatabr.BaseStream.Position < sigDatabr.BaseStream.Length)
                        {

                            readLen = sigDatabr.Read(bfwbuff, 0, hframe.bytesNumOneCircle);
                            if (readLen == hframe.bytesNumOneCircle)
                            {
                                form.mdataqueue.Enqueue(bfwbuff);
                            }
                            else {
                                continue;
                            }

                        }
                        else {
                            bReadDone = true;
                            break;
                        }

                    }
                    if (bQuit || bReadDone)
                    {
                        break;
                    }
                    //System.Threading.Thread.Sleep(1000);
                    //continue;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally {
                
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
            Form1 form = e.Argument as Form1;

            int sumlen = hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum;
            int sumsz = sizeof(float) * sumlen;
            int datasz = sizeof(float) * hframe.iqDotNum;

            IntPtr hAbsCorSumSt = (IntPtr)null;
            IntPtr hAbsCorSumSnd = (IntPtr)null;

            IntPtr hglobalDataSt = (IntPtr)null;
            IntPtr hglobalDataSnd = (IntPtr)null;
            try
            {

                //float[] absCorSumSt = new float[hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum];
                //float[] absCorSumSnd = new float[hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum];

                //Ipp32fc[] IpcDataSt = new Ipp32fc[hframe.sampleNumOneCircle];
                //Ipp32fc[] IpcDataSnd = new Ipp32fc[hframe.sampleNumOneCircle];
                //unmanaged buffer


                hAbsCorSumSt = Marshal.AllocHGlobal(sumsz);
                hAbsCorSumSnd = Marshal.AllocHGlobal(sumsz);

                hglobalDataSt = Marshal.AllocHGlobal(datasz);
                hglobalDataSnd = Marshal.AllocHGlobal(datasz);

                float fmaxst = 0.0f, fmaxsnd = 0.0f;
                int iIndxst = -1, iIndxsnd = -1, iCorrectIdx = -1;

                int hlen = hframe.HFrameTraceRghtOffset + hframe.HFrameTraceLftOffset;
                int htraceSumLen = hlen - MarineHFrame.ccorreCodeNum;
                IntPtr hAbsCorSumTrace = Marshal.AllocHGlobal(htraceSumLen);
                int indxTrace = -1;
                float maxTrace = -1.0f;
                while (true)
                {
                    while (mdataqueue.Count > 0)
                    {
                        if (bQuit)
                        {
                            break;
                        }
                        if (mdataqueue.Count < rtdef.TWO)
                        {
                            Console.WriteLine("已处理完数据,等待下一批数据");
                            System.Threading.Thread.Sleep(1000);
                            continue;
                        }
                        byte[] st;
                        mdataqueue.TryDequeue(out st);
                        //GCHandle pinnedObj = GCHandle.Alloc(anObj, GCHandleType.Pinned);
                        Marshal.Copy(st, 0, hglobalDataSt, hframe.bytesNumOneCircle);

                        //同步操作 寻找H帧的位置
                        #region 
                        if (hframe.HFrameSyncState == false)
                        {
                            byte[] snd;
                            mdataqueue.TryDequeue(out snd);
                            Marshal.Copy(snd, 0, hglobalDataSnd, hframe.bytesNumOneCircle);
                            GetHFrameLocation((Ipp32fc*)hglobalDataSt, hframe.sampleNumOneCircle, (float*)hAbsCorSumSt, hframe.slidelen, ref fmaxst, ref iIndxst);
                            GetHFrameLocation((Ipp32fc*)hglobalDataSnd, hframe.sampleNumOneCircle, (float*)hAbsCorSumSnd, hframe.slidelen, ref fmaxsnd, ref iIndxsnd);
                            if (iIndxsnd < 0 || iIndxst < 0)
                            {
                                //未找到H帧
                                continue;
                            }
                            if (Math.Abs(iIndxsnd - iIndxst) < MarineHFrame.crange)
                            {
                                iCorrectIdx = iIndxsnd;
                                //正确
                            }
                            else {
                                GetCorrectHIndx((float*)hAbsCorSumSt, sumlen, iIndxst, (float*)hAbsCorSumSnd, sumlen, iIndxsnd, &iCorrectIdx);
                            }

                            if (hframe.HFrameLocation < 0)//第一次寻找
                            {
                            }
                            else {
                                if (Math.Abs(hframe.HFrameLocation - iCorrectIdx) > MarineHFrame.crange)//H帧寻找不正确
                                {
                                    hframe.HFrameSyncState = false;
                                    hframe.HSyncTimes = MarineHFrame.cHSyncTimes;
                                }
                                else {
                                    hframe.HSyncTimes--;
                                    if (hframe.HSyncTimes <= 0)
                                    {
                                        hframe.HFrameSyncState = true;
                                    }
                                }

                            }
                            hframe.HFrameLocation = iCorrectIdx;
                        }
                        #endregion
                        //跟踪H帧位置
                        #region 
                        else {
                            //H帧 寻找的起始位置
                            int hstartIndx = hframe.HFrameLocation - hframe.HFrameTraceLftOffset <= 0 ? 0 : hframe.HFrameLocation - hframe.HFrameTraceLftOffset;
                            GetHFrameLocation((Ipp32fc*)hglobalDataSt + hstartIndx, hlen, (float*)hAbsCorSumTrace, htraceSumLen, ref maxTrace, ref indxTrace);
                            //跟踪失败 已失步
                            if (maxTrace < 0 || indxTrace < 0)
                            {
                                hframe.HFrameSyncState = false;
                                hframe.HSyncTimes = MarineHFrame.cHSyncTimes;
                            }
                            else {
                                hframe.HFrameSyncState = true;
                                hframe.HFrameLocation = hstartIndx + indxTrace;
                            }
                        }
                        #endregion
                        Console.WriteLine(hframe.HFrameLocation);
                    }
                    if (bQuit)
                    {
                        break;
                    }
                    if (mdataqueue.Count == 0)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
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

            }

        }
        private void bgSyncHFrameWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) { }
        private void bgSyncHFrameWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) { }


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
            //Ipp32fc* ippsMalloc_32fc(int len);
            Ipp32fc* pSrcConj = sp.ippsMalloc_32fc(srclen);
            sp.ippsZero_32fc(pSrcConj, srclen);
            //求pSrc的共轭
            //IppStatus ippsConj_32fc(const Ipp32fc* pSrc, Ipp32fc* pDst, int len);
            sp.ippsConj_32fc(pSrc, pSrcConj, srclen);
            //IppStatus ippsMul_32fc(const Ipp32fc* pSrc1, const Ipp32fc* pSrc2, Ipp32fc*pDst, int len);
            sp.ippsMul_32fc(pSrc + 1, pSrcConj, pDst, srclen - 1);
            sp.ippsFree(pSrcConj);
            return 0;
        }

        //滑动匹配H帧
        unsafe int SlideMatchHFrame(Ipp32fc* pSrc, int slen, Ipp32fc* pCorrCode, int corrLen, float* pAbsSum, int absSumLen)
        {

            //Ipp32fc* ippsMalloc_32fc(int len);
            Ipp32fc* ptmpSlide = sp.ippsMalloc_32fc(corrLen);
            //Ipp32f *pAbsCorreSum = ippsMalloc_32f(slen - corrLen);
            Ipp32fc* pCorreSum = sp.ippsMalloc_32fc(absSumLen);
            Ipp32fc* pDst = sp.ippsMalloc_32fc(corrLen);
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

            sp.ippsFree(ptmpSlide);
            sp.ippsFree(pCorreSum);
            sp.ippsFree(pDst);
            return 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Marshal.FreeHGlobal(hglobalCorre);
        }

        // 通过对两个循环帧的比对 找出正确的H帧位置
        unsafe int GetCorrectHIndx(float* pAbsSumSt, int lenSt, int indxSt, float* pAbsSumSnd, int lenSnd, int indxSnd, int* pCorrectIndx)
        {
            bool isSatisfyHCon = false, search_result = false;
            float fmaxCorrect;
            int iCorrectIdx;
            int llen = (int)hframe.sampleNumOneCircle - MarineHFrame.ccorreCodeNum - indxSt - 2 * MarineHFrame.ccmpOffset;

            //h未寻找到 假设第一个寻找正确
            //第二个往前跳22帧 此处简单处理 从开始处寻找
            sp.ippsMaxIndx_32f(pAbsSumSnd, indxSnd - 2 * MarineHFrame.ccmpOffset, &fmaxCorrect, &iCorrectIdx);
            //判断最大值是否满足H帧 规律性条件
            PeakVerify(pAbsSumSnd, lenSnd, fmaxCorrect, iCorrectIdx, &isSatisfyHCon);
            if (isSatisfyHCon && Math.Abs(iCorrectIdx - indxSt) < MarineHFrame.crange)
            {
                //寻找正确
                search_result = true;
                *pCorrectIndx = iCorrectIdx;
            }
            else {

                int startidx = indxSnd + 2 * MarineHFrame.ccmpOffset;
                sp.ippsMaxIndx_32f(pAbsSumSnd + startidx, llen, &fmaxCorrect, &iCorrectIdx);
                //判断最大值是否满足H帧 规律性条件
                PeakVerify(pAbsSumSnd, lenSnd, fmaxCorrect, iCorrectIdx, &isSatisfyHCon);
                if (isSatisfyHCon && Math.Abs(iCorrectIdx - indxSt) < MarineHFrame.crange)
                {
                    //寻找正确
                    search_result = true;
                    *pCorrectIndx = iCorrectIdx;
                }
            }
            if (!search_result)// 假设第二个寻找正确
            {
                //第二个往前跳22帧 此处简单处理 从开始处寻找
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
                    int startidx = indxSt + 2 * MarineHFrame.ccmpOffset;
                    sp.ippsMaxIndx_32f(pAbsSumSt + startidx, llen, &fmaxCorrect, &iCorrectIdx);
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
            return 0;
        }

        // 对波峰的判断
        unsafe int PeakVerify(float* pAbsSum, int sumlen, float fmax, int peakIndx, bool* presult)
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
            return 0;
        }

    }
}
