using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarineSignalApp.MarineEntity
{
    public class MarineSFrame : MarineFrame
    {

        //S帧训练码
        public byte[] mSTrainedCode = { };

        public MarineSFrame() { }

        public MarineSFrame(FramType frametype):base(frametype) {

        }
    }
}
