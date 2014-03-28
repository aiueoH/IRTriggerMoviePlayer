using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace DirectionRecognition
{
    public abstract class DR
    {
        public enum Direction
        {
            Right,
            Reverse,
            Exception,
        }

        public abstract Direction Detect(Image<Bgr, byte> img);
    }
}
