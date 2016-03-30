using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarineSignalApp
{
    public class MarineHFrame
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


        public MarineHFrame()
        {
            sampleNumOneCircle = (int)Math.Ceiling(dlSampleNumOneCircle);
            iqDotNum = sampleNumOneCircle * 2;
            bytesNumOneCircle = sizeof(float) * iqDotNum;
            sampleNumOneCircle = (int)Math.Ceiling(dlSampleNumOneCircle);
            ezeroFrameDotNum = (int)Math.Ceiling(80 * 120 / 26.0 * 625);
            ttwoFrameDotNum = (int)Math.Ceiling(22 * 120 / 26.0 * 625);
            dotNumOneFrame = (int)Math.Ceiling(120 / 26 * 625.0);
        }
    }
}
