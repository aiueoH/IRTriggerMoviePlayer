using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace DirectionRecognition
{
    public class BinaryHistogram : DR
    {
        public struct BinaryHistogramSetting
        {
            public Rectangle LeftBottomROI { get; set;}
            public Rectangle LeftTopROI { get; set; }
            public Rectangle RightBottomROI { get; set; }
            public Rectangle RightTopROI { get; set; }
            public double HueMinA { get; set; }
            public double HueMaxA { get; set; }
            public double HueMinB { get; set; }
            public double HueMaxB { get; set; }
            public int ED { get; set; }
        }

        private const double EXCEPTION_RANGE = 80;

        private Rectangle _lb, _lt, _rb, _rt;
        private double _minA, _maxA, _minB, _maxB;
        private double _exceptionRange = EXCEPTION_RANGE;
        private int _ed;

        public BinaryHistogram(Rectangle lb, Rectangle lt, Rectangle rb, Rectangle rt, double minA, double maxA, double minB, double maxB, int ED)
        {
            _lb = lb;
            _lt = lt;
            _rb = rb;
            _rt = rt;
            _minA = minA;
            _maxA = maxA;
            _minB = minB;
            _maxB = maxB;
            _ed = ED;
        }

        public BinaryHistogram(BinaryHistogramSetting setting)
        {
            _lb = setting.LeftBottomROI;
            _lt = setting.LeftTopROI;
            _rb = setting.RightBottomROI;
            _rt = setting.RightTopROI;
            _minA = setting.HueMinA;
            _maxA = setting.HueMaxA;
            _minB = setting.HueMinB;
            _maxB = setting.HueMaxB;
            _ed = setting.ED;
        }

        public override Direction Detect(Image<Bgr, byte> img)
        {
            int bCount = 0, tCount = 0;
            Image<Hsv, byte> imgHsv = img.Convert<Hsv, byte>();

            bCount += countHue(imgHsv, _lb, _minA, _maxA, _minB, _maxB, _ed);
            bCount += countHue(imgHsv, _rb, _minA, _maxA, _minB, _maxB, _ed);
            tCount += countHue(imgHsv, _lt, _minA, _maxA, _minB, _maxB, _ed);
            tCount += countHue(imgHsv, _rt, _minA, _maxA, _minB, _maxB, _ed);

            if (Math.Abs(bCount - tCount) < _exceptionRange)
                return Direction.Exception;
            else
                return (bCount > tCount) ? Direction.Reverse : Direction.Right;
        }

        private int countHue(Image<Hsv, byte> img, Rectangle roi, double min1, double max1, double min2, double max2, int ED)
        {
            img.ROI = roi;
            int c = count255(hueToBin(img, min1, max1, min2, max2).Erode(ED).Dilate(ED));
            img.ROI = new Rectangle();
            return c;
        }

        private int countHue(Image<Hsv, byte> img, double min1, double max1, double min2, double max2, int ED)
        {
            return count255(hueToBin(img, min1, max1, min2, max2).Erode(ED).Dilate(ED));
        }

        private int count255(Image<Gray, byte> img)
        {
            int count = 0;
            for (int i = 0; i < img.Height; i++)
                for (int j = 0; j < img.Width; j++)
                    if (img[i, j].Intensity == 255)
                        count++;
            return count;
        }

        private Image<Gray, byte> hueToBin(Image<Hsv, byte> img, double min1, double max1, double min2, double max2)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(img.Width, img.Height);
            for (int i = 0; i < img.Height; i++)
                for (int j = 0; j < img.Width; j++)
                {
                    double h = img[i, j].Hue;
                    byte g = 0;
                    if ((h >= min1 && h <= max1) || (h >= min2 && h <= max2))
                        g = 255;
                    result.Data[i, j, 0] = g;
                }
            return result;
        }
    }
}
