using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarineSignalApp.MarineTool
{
    public class MarineCommon
    {
        #region field
        //采样速率 单位 KHz
        private long mSampleRate = 625 * 1000;
        //信道带宽 单位K
        private int mChannelBW = 200;
        //比特率 K
        private double mBitRate = 1625 / 6.0;
        //波特率（符号速率） KBd
        private double mBaudRate = 1625 / 12.0 ;
        //采样倍数（内插倍数）
        private int mSampleTime =  3;
        //信号起始频率
        private int mStartFre = 1525;
        //信号结束频率
        private int mEndFre = 1525;
        #endregion
        #region method
        #endregion
         
        #region properity
        public long MSampleRate
        {
            get
            {
                return mSampleRate;
            }

            set
            {
                mSampleRate = value;
            }
        }

        public int MChannelBW
        {
            get
            {
                return mChannelBW;
            }

            set
            {
                mChannelBW = value;
            }
        }

        public double MBitRate
        {
            get
            {
                return mBitRate;
            }

            set
            {
                mBitRate = value;
            }
        }

        public double MBaudRate
        {
            get
            {
                return mBaudRate;
            }

            set
            {
                mBaudRate = value;
            }
        }

        public int MSampleTime
        {
            get
            {
                return mSampleTime;
            }

            set
            {
                mSampleTime = value;
            }
        }

        public int MStartFre
        {
            get
            {
                return mStartFre;
            }

            set
            {
                mStartFre = value;
            }
        }

        public int MEndFre
        {
            get
            {
                return mEndFre;
            }

            set
            {
                mEndFre = value;
            }
        }
        #endregion

        #region method
        // y = 1525.1 + 0.2 * n  ,n取自[0,169]
        //频率点生成方法
        public void ProduceFreDot_1(float []fredot) {
            for (int i = 0; i< fredot.Length;i++) {
                fredot[i] = 1525.1f + 0.2f * i;
            }

        }
        // y = 1525.1 + 0.2 * n + 0.075  ,n取自[0,168]
        public void ProduceFreDot_2(float[] fredot)
        {
            for (int i = 0; i < fredot.Length; i++)
            {
                fredot[i] = 1525.1f + 0.2f * i + 0.075f;
            }

        }
        #endregion
    }
}
