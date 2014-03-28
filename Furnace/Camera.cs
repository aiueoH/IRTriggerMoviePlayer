using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Structure;

using System.Windows.Forms;
using System.Drawing;
using DirectionRecognition;

namespace Furnace
{
    public class Camera
    {
        private const string PICTURE_FOLDER = "Picture";

        private Capture _capture = null;
        private DR _dr = null;

        private int _CUT_LEFT_UP_X = 60;
        private int _CUT_LEFT_UP_Y = 65;
        private int _CUT_RIGHT_DOWN_X = 580;
        private int _CUT_RIGHT_DOWN_Y = 420;
        private double _SCALE = 1.3;
        private double _ROTATE = 0;
        private bool _SAVE_PICTURE;

        public Camera(int CUT_LEFT_UP_X, 
            int CUT_LEFT_UP_Y, 
            int CUT_RIGHT_DOWN_X, 
            int CUT_RIGHT_DOWN_Y,
            double SCALE,
            double ROTATE,
            bool SAVE_PICTURE,
            BinaryHistogram.BinaryHistogramSetting setting)
        {
            _CUT_LEFT_UP_X = CUT_LEFT_UP_X;
            _CUT_LEFT_UP_Y = CUT_LEFT_UP_Y;
            _CUT_RIGHT_DOWN_X = CUT_RIGHT_DOWN_X;
            _CUT_RIGHT_DOWN_Y = CUT_RIGHT_DOWN_Y;

            _SCALE = SCALE;
            _ROTATE = ROTATE;

            _SAVE_PICTURE = SAVE_PICTURE;

            _capture = new Capture();
            _dr = new BinaryHistogram(setting);
            
        }

        public void Close()
        {
            _capture.Dispose();
        }

        public Image<Bgr, Byte> GetPhoto()
        {
            _capture.QueryFrame();
            Image<Bgr, Byte> frame = _capture.QueryFrame();
            if (_SAVE_PICTURE)
                SavePicture(frame);
            frame = frame.Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            DR.Direction d = _dr.Detect(frame);
            if (d == DR.Direction.Exception)
                return null;
            //Image<Bgr, Byte> tmp = new Image<Bgr, byte>(640, 640);
            //tmp.ROI = new Rectangle(0, tmp.Height / 2 - frame.Height / 2 - 1, frame.Width, frame.Height);
            //frame.CopyTo(tmp); 
            //tmp.ROI = new Rectangle();
            //tmp = tmp.Rotate(90.0, new Bgr(0, 0, 0));
            //tmp.ROI = new Rectangle(tmp.Width / 2 - frame.Height / 2 - 1, 0, frame.Height, frame.Width);
            //tmp = tmp.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
            //Image<Bgr, Byte> result = new Image<Bgr, byte>(480, 640);
            //tmp.CopyTo(result);
            frame = Cut(frame);
            frame = frame.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
            if (d == DR.Direction.Reverse)
                frame = frame.Rotate(180, new Bgr(0, 0, 0));
            frame = frame.Resize(_SCALE, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            frame = frame.Rotate(_ROTATE, new Bgr(0, 0, 0));
            return frame;
        }

        public Image<Bgr, byte> Cut(Image<Bgr, byte> ori)
        {
            int cutW = _CUT_RIGHT_DOWN_X - _CUT_LEFT_UP_X;
            int cutH = _CUT_RIGHT_DOWN_Y - _CUT_LEFT_UP_Y;
            Image<Bgr, byte> tmp = ori.Clone();
            tmp.ROI = new Rectangle(_CUT_LEFT_UP_X, _CUT_LEFT_UP_Y, cutW, cutH);
            return tmp.Resize(ori.Width, ori.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        private void SavePicture(Image<Bgr, Byte> img)
        {
            if (!System.IO.Directory.Exists(PICTURE_FOLDER))
                System.IO.Directory.CreateDirectory(PICTURE_FOLDER);
            img.Save(PICTURE_FOLDER + "/" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".bmp");
        }
    }
}
