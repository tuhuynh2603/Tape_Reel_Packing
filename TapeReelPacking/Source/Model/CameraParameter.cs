using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapeReelPacking.Source.Model
{

    public class CameraParameter
    {
        public bool softwareTrigger { set; get; }
        public float exposureTime { set; get; }
        public float frameRate { set; get; }
        public float gain { set; get; }
        //public CameraParameter(bool softwareTrigger, float exposureTime, float frameRate, float gain)
        //{
        //    this.softwareTrigger = softwareTrigger;
        //    this.exposureTime = exposureTime;
        //    this.frameRate = frameRate;
        //    this.gain = gain;
        //}
    }
}
