using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarineSignalApp
{
    public class RtHFrame
    {
        //一个循环的采样点数 每102帧一循环 每帧时长 120/26ms 采样率为625KHz
        public const double samNumOneCircle = 102 * 120 / 26.0 * 625;
        //向上取整
        public readonly int sampleNumOneCircle;
        //每次读取数据的长度 每个采样点由IQ 两个数据组成
        public readonly int IqDotNum;
        //一帧的点数
        public readonly int dotNumOneFrame;
        //22帧 共22*120/26.0 *625;
        public readonly int ttwoFrameDotNum;
        //80帧 共22*120/26.0 *625;
        public readonly int ezeroFrameDotNum;
        public readonly int bytesNumOneCircle;
        //相关码长度
        public const int correCodeNum = 372;

        public const int c_cmpOffset = 4;
        public const float c_fthreshold = 1.2f;

        //匹配范围
        public  const int c_range = 1000;
        //22帧
        public const int c_ttwo = 22;
        //80帧
        public const int c_ezero = 80;


       public RtHFrame()
        {
            sampleNumOneCircle = (int)Math.Ceiling(samNumOneCircle);
            IqDotNum = sampleNumOneCircle * 2;
            bytesNumOneCircle = sizeof(float) * IqDotNum;
            sampleNumOneCircle = (int)Math.Ceiling(samNumOneCircle);
            ezeroFrameDotNum = (int)Math.Ceiling(80 * 120 / 26.0 * 625);
            ttwoFrameDotNum = (int)Math.Ceiling(22 * 120 / 26.0 * 625);
            dotNumOneFrame = (int)Math.Ceiling(120 / 26 * 625.0);
        }
    }
}
