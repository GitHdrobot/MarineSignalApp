
using System;
using MarineSignalApp.MarineEntity;
using MarineSignalApp.MarineTool;

namespace MarineSignalApp
{
  
    public class MarineHFrame: MarineFrame 
    {
        //一个循环的采样点数 每102帧一循环 每帧时长 120/26ms 采样率为625KHz
        private const double dlSampleNumOneCircle = 102 * 120 / 26.0 * 625;
        //相关码长度
        public const int ccorreCodeNum = 372;
        //每个样点的字节数
        public const int cbytesOfOneSample = 2 * sizeof(float);
        //相干码数据长度
        public const int cbytesOfCorreCode = cbytesOfOneSample * ccorreCodeNum;
        //相干后 判断是否为H帧时偏移量
        public const int ccmpOffset = 4;
        //判断H帧位置时的门限值  相干后求得最大值与偏移ccmpOffset个位置处 进行比较
        public const float cflthreshold = 1.2f;
        //匹配范围
        public const int crange = 1000;
        //22帧
        public const int cttwo = 22;
        //80帧
        public const int cezero = 80;
        //H帧同步次数
        public const int cHSyncTimes = 2;
        //H帧相关间隔 >= 1
        public const int cHSpace = 8;

        //向上取整
        public readonly int sampleNumOneCircle;
        //每次读取数据的长度 每个采样点由IQ 两个数据组成 I 和 Q的总点数
        public readonly int iqDotNum;
        //一帧的点数
        public readonly int dotNumOneFrame;
        //22帧 共22*120/26.0 *625;
        public readonly int ttwoFrameDotNum;
        //80帧 共22*120/26.0 *625;
        public readonly int ezeroFrameDotNum;
        //一帧的字节数 
        public readonly int bytesNumOneCircle;
        //H帧相关时 滑动的长度
        public readonly int slidelen;


        //H帧同步状态
        public bool HFrameSyncState;
        //H帧同步次数
        public int HSyncTimes;
        //H帧当前帧的位置 同步操作时更新该位置
        public int HFrameLocation;
        ////H帧前一帧的位置 同步操作时更新该位置
        //public int HFramePreLocation;

        //跟踪时H帧时 为确保取到H帧的数据 左右偏移xxoffset个样点
        public readonly int HFrameTraceRghtOffset ;
        public readonly int HFrameTraceLftOffset;
        //存放相位

        //平均相位 ~ 频偏
        public double avgPhase;
        ////相位和 ~ 频偏
        //public float sumPhase;
        //帧计数 寻找成功的帧数
        public long sucssFramNum;
        //H帧相位方差
        public double variancePhase;

        //H帧相干码 训练码
        public readonly int[] cHTrainedCode = {
          0,0,0,
          1,1,1,0,0,0,0,0,1,1,0,0,0,1,0,1,0,1,1,0,0,1,1,0,0,1,0,1,
          1,1,1,1,1,0,1,1,1,1,0,0,1,1,0,1,1,1,0,1,1,1,0,0,1,0,1,0,1,
          0,0,1,0,1,0,0,0,1,0,0,1,0,1,1,0,1,0,0,0,1,1,0,0,1,1,1,0,0,
          1,1,1,1,0,0,0,1,1,0,1,1,0,0,0,0,1,0,0,0,1,0,1,1,1,0,1,0,1,
          1,1,1,0,1,1,0,1,1,1,1,1,0,0,0,0,1,1,0,1,0,0,1,1,0,1,0,
          1,0,0 };
        //H帧构造函数
        public MarineHFrame(FramType frametype):base (frametype)
        {
            sampleNumOneCircle = (int)Math.Ceiling(dlSampleNumOneCircle);
            iqDotNum = sampleNumOneCircle * 2;
            bytesNumOneCircle = sizeof(float) * iqDotNum;
            sampleNumOneCircle = (int)Math.Ceiling(dlSampleNumOneCircle);
            ezeroFrameDotNum = (int)Math.Ceiling(80 * 120 / 26.0 * 625);
            ttwoFrameDotNum = (int)Math.Ceiling(22 * 120 / 26.0 * 625);
            dotNumOneFrame = (int)Math.Ceiling(120 / 26 * 625.0);
            HFrameSyncState = false;
            HFrameLocation = -1;
            HSyncTimes = 2;
            slidelen = sampleNumOneCircle - MarineHFrame.ccorreCodeNum;
            HFrameTraceLftOffset = ccorreCodeNum;
            HFrameTraceRghtOffset = 2*ccorreCodeNum;

            avgPhase = 0.0;
            sucssFramNum = 0;
        }
        //相干码生成，未实现
        public void ProduceCorreCode(int[]correToCoded, int[]correcode,ref int[]correI,ref int[] correQ) {
            for ( int i=0;i<correcode.Length;i++) {
                int idx =  (int)Math.Floor(i/2.0);
                correToCoded[i + 1] = (1 - 2 * correcode[i + 1]) * (-1) ^ idx;
            }
           for (int i =0;i<correcode.Length/2;i++) {
                    correI[i] = correToCoded[2 * i + 1];
                    correQ[i] = correToCoded[2*i];
                }

        }
    }
}
