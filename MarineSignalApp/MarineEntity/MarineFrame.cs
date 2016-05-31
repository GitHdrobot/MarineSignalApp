using MarineSignalApp.MarineTool;
using System;

namespace MarineSignalApp.MarineEntity
{
    
    public class MarineFrame
    {
        protected readonly int mTimeSlotNum = 8;//一帧的时隙数
        protected readonly double mTimePerTs = 15/26.0;//每个时隙的时间长度
                                                 
        public enum FramType { CONTROL_FRAME, MESSAGE_FRAME, SYNCHRONOUS_FRAME };//帧类型  控制帧、消息帧、同步帧
        //一个循环的采样点数 每102帧一循环 每帧时长 120/26ms 采样率为625KHz
        private const double mdlSampleNum = 102 * 120 / 26.0 * 625;
        //一个循环的采样点数  向上取整
        public readonly int mSampleNum = (int)Math.Ceiling(mdlSampleNum);
        private FramType mFramType;//帧类型 控制帧、消息帧、同步帧
        private long mFrameLocation;//帧的位置
        private long mFrameNumber;//帧号

        public MarineFrame(FramType frameType) {
            mFramType = frameType;
        }
        public MarineFrame()
        {
        }

        public FramType MFramType
        {
            get{return mFramType;}
            set{mFramType = value;}
        }

        public long MFrameLocation {
            get { return mFrameLocation; }
            set { mFrameLocation = value; }
        }

        public long MFrameNumber {
            get { return mFrameNumber; }
            set { mFrameNumber = value; }
        }


    }

}
