using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        //相关码长度
        public const int CORRE_LEN = 372;
        public const int BYTES_OF_CORRE = 8* CORRE_LEN;

        public RtHFrame hframe;

        // protected OpenFileDialog fileDlg;
        //信号文件名
        protected readonly String fileSigName = "F:\\海事卫星\\Marinsig\\1.5327006GHz_500k_东偏南.bin";
        //相关码文件名
        protected readonly String fileCorCodeName = "data\\correlator.bin";
        protected FileStream sigDatafs;
        protected BinaryReader sigDatabr;

        protected bool bReadDone, bQuit;

        public ConcurrentQueue<byte[]> mdataqueue;
        //相干码矩阵
        Ipp32fc[] mArrayCorrCode;


        //Windows 组件 初始化
        public Form1()
        {
            InitializeComponent();
            InitialConfig();
        }

        private void checkedListBoxControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (BgFetchDataWorker.IsBusy != true)
            {
                // 启动异步操作
                BgFetchDataWorker.RunWorkerAsync(this);
            }
            this.simpleButton2.Enabled = false;
            this.simpleButton3.Enabled = true;
            bReadDone = true;
            sigDatabr.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        //处理读取到的数据
        public unsafe IppStatus Rt_ProcessData()
        {
            float* pfrbuff = sp.ippsMalloc_32f(hframe.IqDotNum * 2);
            Ipp32fc* pIpcDataSt = sp.ippsMalloc_32fc(hframe.sampleNumOneCircle);
            Ipp32fc* pIpcDataSnd = sp.ippsMalloc_32fc(hframe.sampleNumOneCircle);
            float fmaxst, fmaxsnd, fmaxCorrect;
            int iIndxst = -1, iIndxsnd = -1, iCorrectIdx;
            float* pAbsCorSumSt = sp.ippsMalloc_32f(hframe.sampleNumOneCircle - RtHFrame.correCodeNum);
            float* pAbsCorSumSnd = sp.ippsMalloc_32f(hframe.sampleNumOneCircle - RtHFrame.correCodeNum);

            while (true)
            {

            }
            //while (true)
            //{

            //    while (pdlg->is_running)
            //    {
            //        if (pdlg->bQuit)
            //        {
            //            break;
            //        }
            //        //读取2倍IqDotNum个IQ数据 即每次处理两个102帧数据
            //        BOOL rslt = pdlg->m_memory.ReadFromMemory(pfrbuff, IqDotNum * 2);
            //        if (!rslt)
            //        {
            //            Sleep(10);
            //        }
            //        //拷贝到ipp复数缓冲区 构造复数对
            //        memcpy(pIpcDataSt, pfrbuff, IqDotNum * sizeof(float));
            //        memcpy(pIpcDataSnd, pfrbuff + IqDotNum, IqDotNum * sizeof(float));
            //        pdlg->GetHFrameLocation(pIpcDataSt, c_perDataLen, pAbsCorSumSt, c_perDataLen - c_correLen, &fmaxst, &iIndxst);
            //        pdlg->GetHFrameLocation(pIpcDataSnd, c_perDataLen, pAbsCorSumSnd, c_perDataLen - c_correLen, &fmaxsnd, &iIndxsnd);
            //        if (abs(iIndxsnd - iIndxst) < c_range)
            //        {
            //            iCorrectIdx = iIndxsnd;
            //            //正确
            //        }
            //        else {
            //            pdlg->GetCorrectHIndx(pAbsCorSumSt, c_perDataLen - c_correLen, iIndxst, pAbsCorSumSnd, c_perDataLen - c_correLen, iIndxsnd, &iCorrectIdx);
            //        }


            //    }
            //    if (pdlg->bQuit)
            //    {
            //        break;
            //    }
            //    Sleep(10);
            //}
            sp.ippsFree(pfrbuff);
            sp.ippsFree(pIpcDataSt);
            sp.ippsFree(pIpcDataSnd);
            sp.ippsFree(pAbsCorSumSt);
            sp.ippsFree(pAbsCorSumSnd);
            return 0;
        }


        //进行内存申请 和 初始化
        public void Rt_MemInitial()
        {
            try
            {

            }
            catch (InsufficientMemoryException e)
            {
                Console.Write(e.StackTrace);
            }
        }

        public void Rt_RetureBuffer()
        {


        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                fileDlg = new OpenFileDialog();
                fileDlg.Filter = "data file (*.bin)|*.dat|All files (*.*)|*.*";
                fileDlg.ShowReadOnly = true;
                DialogResult r = fileDlg.ShowDialog();
                if (r == DialogResult.OK)//打开成功
                {
                    sigDatafs = new FileStream(fileDlg.FileName, FileMode.Open);
                    sigDatabr = new BinaryReader(sigDatafs);
                    this.textEdit3.Text = fileDlg.FileName;
                }
            }
            catch (Exception)
            {

                Console.Write("open error");
            }

        }

        //进行必要的初始化工作
        public void InitialConfig()
        {
            Rt_MemInitial();
            //创建一个HFrame实例
            hframe = new RtHFrame();
            //buff
            mdataqueue = new ConcurrentQueue<byte[]>();
            mArrayCorrCode = new Ipp32fc[BYTES_OF_CORRE];
            //IntPtr pcorre = &mArrayCorrCode;
            byte []tb = new byte[BYTES_OF_CORRE*8];
           
            readCorreCode(ref tb);
           // Marshal.Copy(tb, 0, (IntPtr)mArrayCorrCode, BYTES_OF_CORRE);
            bReadDone = false;
            bQuit = false;
            this.simpleButton3.Enabled = false;
            this.simpleButton2.Enabled = true;
            this.textEdit1.Text = hframe.bytesNumOneCircle + "";
            this.textEdit2.Text = 0.ToString();
            this.textEdit3.Text = fileSigName;
        }
        //读取相干码文件
        public void readCorreCode(ref byte[] buffer) {
            FileStream fscorre = new FileStream(fileCorCodeName,FileMode.Open);
            BinaryReader brcorre = new BinaryReader(fscorre);
            brcorre.Read(buffer, 0,BYTES_OF_CORRE);

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
            byte[] bfwbuff = new byte[hframe.bytesNumOneCircle];
            while (true)
            {

                while (bReadDone)
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
                    if (sigDatabr.BaseStream.Position < sigDatabr.BaseStream.Length )
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
                if (bQuit)
                {
                    break;
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        //更新进度
        private void BgFetchDataWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

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
            bReadDone = false;
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



    }
}
