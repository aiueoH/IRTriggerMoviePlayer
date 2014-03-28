using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Diagnostics;
using System.Threading;

using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace Furnace
{
    public class Player
    {
        private const float LIEDOWN_WIDTH_SPACE = 0.1f;
        private const float LIEDOWN_HEIGHT_SPACE = 0.6f;
        private const byte DEFAULT_MASKCOLOR = 0;
        private const int DEFAULT_LIEDOWN_FRAMETIME = 33;
        private const int DEFALUT_TRANSPARENT_COLOR_B = 255;
        private const int DEFALUT_TRANSPARENT_COLOR_G = 255;
        private const int DEFALUT_TRANSPARENT_COLOR_R = 255;
        private const int GP_POS_X = 200;
        private const int GP_POS_Y = 80;

        private const int FRAME_GAP = 4;

        private int _BURNFRAME_COUNT = 401;
        private int _PREFRAME_COUNT = 100;

        private int _BURN_FRAME_TIME = 1;
        private int _PRE_FRAME_TIME = 25;

        private bool _USE_MASK = false;
        //private const int _PRE_FRAME_COUNT = 0;

        private const string ANIMATION_MASK_PATH = @"./Animation/Mask/mask_#####.png";
        private const string ANIMATION_FIRE_PATH = @"./Animation/Fire/fire_#####.png";
        private const string ANIMATION_PREFIRE_PATH = @"./Animation/PreFire/fire_#####.png";

        public delegate void OnFrameUpdateHandler(object sender, EventArgs e);
        public event OnFrameUpdateHandler OnFrameUpdate;
        public delegate void OnPlayerEndHandler(object sender, EventArgs e);
        public event OnPlayerEndHandler OnPlayerEnd;

        //private int _frameWidth = 1024;
        //private int _frameHeight = 760;

        private int _FRMAE_W = 1024;
        private int _FRMAE_H = 760;
        //private int _PAPER_W = 640;
        //private int _PAPER_H = 480;
        //private int _PAPER_X;
        //private int _PAPER_Y;

        private List<Bitmap> _preFrames;
        private ConcurrentQueue<Bitmap> _instantFrames;
        private List<Bitmap> _maskBitmaps;
        private List<Bitmap> _fireBitmaps;
        private Image<Bgr, byte> _goldPaper;

        private bool _isPlaying;

        private Image<Bgra, byte> _currentFrame;
        private Image<Bgr, byte> _blackBackground;

        private Thread _playerThread;
        private Thread _preAniThread;

        public Player(int PRE_FRAME_TIME, int BURN_FRAME_TIME, int PREFRAME_COUNT, int BURNFRAME_COUNT, bool USE_MASK)
        {
            _PRE_FRAME_TIME = PRE_FRAME_TIME;
            _BURN_FRAME_TIME = BURN_FRAME_TIME;
            _PREFRAME_COUNT = PREFRAME_COUNT;
            _BURNFRAME_COUNT = BURNFRAME_COUNT;
            _USE_MASK = USE_MASK;

            _isPlaying = false;
        }

        public void Close()
        {
            if (_playerThread != null)
                _playerThread.Abort();
            if (_preAniThread != null)
                _preAniThread.Abort();
        }

        public void Join()
        {
            if (_playerThread != null)
            {
                _playerThread.Join();
            }
        }

        public void Play()
        {
            if (!_isPlaying)
            {
                _isPlaying = true;
                if (_playerThread != null)
                    _playerThread.Abort();
                _playerThread = new Thread(new ThreadStart(PlayAnimition));
                _playerThread.Start();
            }
        }

        public void PlayPreAni()
        {
            if (_preAniThread != null)
                _preAniThread.Abort();
            _preAniThread = new Thread(new ThreadStart(PlayPreFireAnimation));
            _preAniThread.Start();
        }

        public Image<Bgra, byte> GetFrame()
        {
            return _currentFrame;
        }

        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public void SetGoldPaper(Image<Bgr, byte> goldPaper)
        {
            _goldPaper = goldPaper;
        }

        private void UpdateCurrentFrame(Image<Bgra, byte> frame)
        {
            _currentFrame = frame;
            if (OnFrameUpdate != null)
            {
                OnFrameUpdate(this, null);
            }
        }

        #region Load

        //private void Load()
        //{
        //    //LoadMask();
        //    //LoadFire();
        //    //_frameWidth = _fires.First().Width;
        //    //_frameHeight = _fires.First().Height;
        //}

        //private void LoadMask()
        //{
        //    _maskBitmaps = new List<Bitmap>();

        //    //List<List<Bitmap>> paral = new List<List<Bitmap>>();
        //    //for (int i = 0; i < 4; i++)
        //    //    paral.Add(null);
        //    //DateTime d = DateTime.Now;
        //    //Parallel.For(0, 4, o =>
        //    //{
        //    //    paral[o] = new List<Bitmap>();
        //    //    for (int i = o * 100; i <= o * 100 + 99; i++)
        //    //    {
        //    //        if (i % FRAME_GAP != 0 && i != 400)
        //    //            continue;
        //    //        string file = ANIMATION_MASK_PATH.Replace("#####", i.ToString("00000"));
        //    //        Debug.WriteLine("Load mask : " + file);
        //    //        paral[o].Add((Bitmap)Bitmap.FromFile(file));
        //    //    }
        //    //});
        //    //for (int i = 0; i < 4; i++)
        //    //    _maskBitmaps.AddRange(paral[i]);
        //    //Debug.WriteLine("load mask duration : " + (DateTime.Now - d).TotalMilliseconds);

        //    DateTime d = DateTime.Now;
        //    for (int i = 0; i <= 400; i++)
        //    {
        //        if (i % FRAME_GAP != 0 && i != 400)
        //            continue;
        //        string file = ANIMATION_MASK_PATH.Replace("#####", i.ToString("00000"));
        //        Debug.WriteLine("Load mask : " + file);
        //        _maskBitmaps.Add((Bitmap)Bitmap.FromFile(file));
        //    }
        //    Debug.WriteLine("load mask duration : " + (DateTime.Now - d).TotalMilliseconds);
        //}

        //private void LoadFire()
        //{
        //    _fireBitmaps = new List<Bitmap>();
        //    DateTime d = DateTime.Now;
        //    for (int i = 0; i <= 400; i++)
        //    {
        //        if (i % FRAME_GAP != 0 && i != 400)
        //            continue;
        //        string file = ANIMATION_FIRE_PATH.Replace("#####", i.ToString("00000"));
        //        Debug.WriteLine("Load fire : " + file);
        //        _fireBitmaps.Add((Bitmap)Bitmap.FromFile(file));
        //    }
        //    Debug.WriteLine("load fire duration : " + (DateTime.Now - d).TotalMilliseconds);
        //}

        //private void LoadMask()
        //{
        //    _maskBitmaps = new List<Bitmap>();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        if (i % 70 != 0)
        //            continue;
        //        string file = ANIMATION_MASK_PATH.Replace("#####", i.ToString("00000"));
        //        Debug.WriteLine("Load mask : " + file);
        //        _maskBitmaps.Add(Bitmap.FromFile(file) as Bitmap);
        //    }
        //}

        //private void LoadFire()
        //{
        //    _fireBitmaps = new List<Bitmap>();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        if (i % 70 != 0)
        //            continue;
        //        string file = ANIMATION_FIRE_PATH.Replace("#####", i.ToString("00000"));
        //        Debug.WriteLine("Load fire : " + file);
        //        _fireBitmaps.Add(Bitmap.FromFile(file) as Bitmap);
        //    }
        //}

        #endregion

        private void PlayPreFireAnimation()
        {
            while (!_isPlaying)
                for (int i = 0; i < _PREFRAME_COUNT && !_isPlaying; i++)
                { 
                    using (Bitmap frameBitmap = new Bitmap(_FRMAE_W, _FRMAE_H),
                        fire = (Bitmap)Bitmap.FromFile(ANIMATION_PREFIRE_PATH .Replace("#####", i.ToString("00000"))))
                    {
                        using (Graphics g = Graphics.FromImage(frameBitmap))
                        {
                            g.DrawImage(fire, 0, 0, fire.Width, fire.Height);
                        }
                        using (Image<Bgra, byte> frameImage = new Image<Bgra, byte>(frameBitmap))
                        {
                            UpdateCurrentFrame(frameImage);
                            Thread.Sleep(_PRE_FRAME_TIME);
                        }
                    }
                }
        }

        private void PlayBurnAnimation()
        {
            //-----------hotfix-------------------------
            if (_goldPaper == null)
            {
                Thread.Sleep(500);
                return;
            }
            //-----------hotfix-------------------------
            using (Image<Bgr, byte> paperImage = _goldPaper.Clone())
            {
                int paperW = paperImage.Width;
                int paperH = paperImage.Height;
                int paperX = _FRMAE_W / 2 - paperW / 2;
                int paperY = _FRMAE_H / 2 - paperH / 2;
                for (int i = 0; i < _BURNFRAME_COUNT; i++)
                {
                    using (Bitmap frameBitmap = new Bitmap(_FRMAE_W, _FRMAE_H),
                        paper = paperImage.ToBitmap(),
                        mask = (Bitmap)Bitmap.FromFile(ANIMATION_MASK_PATH.Replace("#####", i.ToString("00000"))),
                        fire = (Bitmap)Bitmap.FromFile(ANIMATION_FIRE_PATH.Replace("#####", i.ToString("00000"))))
                    {
                        using (Graphics g = Graphics.FromImage(frameBitmap))
                        {
                            g.DrawImage(paper, paperX, paperY, paperW, paperH);
                            if (_USE_MASK)
                                g.DrawImage(mask, 0, 0, mask.Width, mask.Height);
                            g.DrawImage(fire, 0, 0, fire.Width, fire.Height);
                        }
                        using (Image<Bgra, byte> frameImage = new Image<Bgra, byte>(frameBitmap))
                        {
                            UpdateCurrentFrame(frameImage);
                            Thread.Sleep(_BURN_FRAME_TIME);
                        }
                    }
                }
            }
        }

        #region Aimation

        private void PlayAnimition()
        {
            PlayBurnAnimation();
            _isPlaying = false;

            PlayPreAni();

            _playerThread = null;
            if (OnPlayerEnd != null)
                OnPlayerEnd(this, null);

        }

        //private void LieDownAnimation(int frmaeTime = DEFAULT_LIEDOWN_FRAMETIME)
        //{
        //    //float rate = 0f;
        //    //while (rate < 1.0f)
        //    //{
        //    //    Image<Bgr, byte> frame = _blackBackground.Clone();
        //    //    Image<Bgr, byte> goldPaper = LieDown(_goldPaper, rate);
        //    //    frame.ROI = new Rectangle(GP_POS_X, GP_POS_Y, goldPaper.Width, goldPaper.Height);
        //    //    goldPaper.CopyTo(frame);
        //    //    frame.ROI = new Rectangle();
        //    //    UpdateCurrentFrame(frame);
        //    //    rate += 0.01f;
        //    //    Thread.Sleep(frmaeTime);
        //    //}
        //}

        //private void BurnAnimation(int frmaeTime = DEFAULT_BURN_FRAME_TIME)
        //{
        //    Image<Bgr, byte> img = _goldPaper.Clone();
        //    for (int i = 0; i < _masks.Count; i++)
        //    {
        //        Image<Gray, byte> mask = _masks[i];
        //        Image<Bgr, byte> fire = _fires[i];
        //        Image<Bgr, byte> frame = _blackBackground.Clone();
        //        Form1.OutputConsole.Text = "Sub Mask : " + i.ToString();
        //        SubMask(img, mask);
        //        Image<Bgr, byte> goldPaper = LieDown(img, 1f);
        //        frame.ROI = new Rectangle(GP_POS_X, GP_POS_Y, goldPaper.Width, goldPaper.Height);
        //        goldPaper.CopyTo(frame);
        //        frame.ROI = new Rectangle();
        //        //Form1.OutputConsole.Text = "Add Fire : " + i.ToString();
        //        //AddFire(frame, fire, 0.5f);
        //        UpdateCurrentFrame(frame);
        //        Thread.Sleep(frmaeTime);
        //    }
        //}

        //private void BurnAnimation(int frmaeTime = _BURN_FRAME_TIME)
        //{
        //    Image<Bgr, byte> img = _goldPaper.Clone();
        //    for (int i = 0; i < _maskBitmaps.Count; i++)
        //    {
        //        Image<Bgr, byte> frame = _blackBackground.Clone();
        //        Bitmap bg = new Bitmap(1024, 760);
        //        Graphics g = Graphics.FromImage(bg);
        //        g.DrawImage(img.ToBitmap(), _PAPER_X, _PAPER_Y, _PAPER_W, _PAPER_H);
        //        g.DrawImage(_maskBitmaps[i], 0, 0, _maskBitmaps[i].Width, _maskBitmaps[i].Height);
        //        g.DrawImage(_fireBitmaps[i], 0, 0, _fireBitmaps[i].Width, _fireBitmaps[i].Height);
        //        g.Dispose();
        //        UpdateCurrentFrame(new Image<Bgra, byte>(bg));
        //        bg.Dispose();
        //        Thread.Sleep(frmaeTime);
        //    }
        //}

        //private void BurnAnimationWithParallel(int frmaeTime = DEFAULT_BURN_FRAME_TIME)
        //{
        //    List<Bitmap> frames = new List<Bitmap>();
        //    List<List<Bitmap>> parallelFrames = new List<List<Bitmap>>();
        //    using (Image<Bgr, byte> paperImage = _goldPaper.Clone())
        //    {
        //        for (int i = 0; i < 4; i++)
        //            parallelFrames.Add(new List<Bitmap>());
        //        Parallel.For(0, 4, p =>
        //        {
        //            for (int i = p * 100; i <= p * 100 + 99; i++)
        //            {
        //                Bitmap frame = new Bitmap(1024, 760);
        //                using (Bitmap paper = paperImage.ToBitmap(),
        //                    mask = (Bitmap)Bitmap.FromFile(ANIMATION_MASK_PATH.Replace("#####", i.ToString("00000"))),
        //                    fire = (Bitmap)Bitmap.FromFile(ANIMATION_FIRE_PATH.Replace("#####", i.ToString("00000"))))
        //                {
        //                    using (Graphics g = Graphics.FromImage(frame))
        //                    {
        //                        g.DrawImage(paper, _maskX, _maskY, _maskWidth, _maskHeight);
        //                        g.DrawImage(mask, 0, 0, mask.Width, mask.Height);
        //                        g.DrawImage(fire, 0, 0, fire.Width, fire.Height);
        //                    }
        //                }
        //                parallelFrames[p].Add(frame);
        //            }
        //        });
        //    }
        //    for (int i = 0; i < 4; i++)
        //        frames.AddRange(parallelFrames[i]);
        //    for (int i = 0; i < frames.Count; i++)
        //    {
        //        using (Bitmap f = frames[i])
        //        using (Image<Bgra, byte> img = new Image<Bgra, byte>(frames[i]))
        //        {
        //            UpdateCurrentFrame(img);
        //            Thread.Sleep(frmaeTime);
        //        }
        //    }
        //}

        //private void BurnAnimationWithParallel(int frmaeTime = _BURN_FRAME_TIME)
        //{
        //    List<Bitmap> frames = new List<Bitmap>();
        //    List<List<Bitmap>> parallelFrames = new List<List<Bitmap>>();
        //    using (Image<Bgr, byte> paperImage = _goldPaper.Clone())
        //    {
        //        for (int i = 0; i < 4; i++)
        //            parallelFrames.Add(new List<Bitmap>());
        //        Parallel.For(0, 4, p =>
        //        {
        //            for (int i = p * 100; i <= p * 100 + 99; i++)
        //            {
        //                Bitmap frame = new Bitmap(1024, 760);
        //                using (Bitmap paper = paperImage.ToBitmap(),
        //                    mask = (Bitmap)Bitmap.FromFile(ANIMATION_MASK_PATH.Replace("#####", i.ToString("00000"))),
        //                    fire = (Bitmap)Bitmap.FromFile(ANIMATION_FIRE_PATH.Replace("#####", i.ToString("00000"))))
        //                {
        //                    using (Graphics g = Graphics.FromImage(frame))
        //                    {
        //                        g.DrawImage(paper, _PAPER_X, _PAPER_Y, _PAPER_W, _PAPER_H);
        //                        g.DrawImage(mask, 0, 0, mask.Width, mask.Height);
        //                        g.DrawImage(fire, 0, 0, fire.Width, fire.Height);
        //                    }
        //                }
        //                parallelFrames[p].Add(frame);
        //            }
        //        });
        //    }
        //    for (int i = 0; i < 4; i++)
        //        frames.AddRange(parallelFrames[i]);
        //    for (int i = 0; i < frames.Count; i++)
        //    {
        //        using (Bitmap f = frames[i])
        //        using (Image<Bgra, byte> img = new Image<Bgra, byte>(frames[i]))
        //        {
        //            UpdateCurrentFrame(img);
        //            Thread.Sleep(frmaeTime);
        //        }
        //    }
        //}

        #endregion

        //private void SubMask(Image<Bgr, byte> img, Image<Gray, byte> mask, byte maskColor = DEFAULT_MASKCOLOR)
        //{
        //    MIplImage imgIpl = img.MIplImage;
        //    MIplImage maskIpl = mask.MIplImage;
        //    unsafe
        //    {
        //        byte* imgPixel = (byte*)imgIpl.imageData;
        //        byte* maskPixel = (byte*)maskIpl.imageData;
        //        for (int h = 0; h < img.Height; h++)
        //        {
        //            for (int w = 0; w < img.Width; w++)
        //                if (maskPixel[w] == maskColor)
        //                {
        //                    int point = w * 3;
        //                    imgPixel[point] = 0;
        //                    imgPixel[point + 1] = 0;
        //                    imgPixel[point + 2] = 0;
        //                }
        //            maskPixel = maskPixel + maskIpl.widthStep;
        //            imgPixel = imgPixel + imgIpl.widthStep;
        //        }
        //    }
        //}

        //private void AddMask(Image<Bgr, byte> img, Image<Bgra, byte> mask)
        //{
        //    for (int y = 0; y < img.Height; y++)
        //    {
        //        for (int x = 0; x < img.Width; x++)
        //        {
        //            double a = mask[y, x].Alpha / 255;
        //            double b = (1 - a) * img[y, x].Blue;
        //            double g = (1 - a) * img[y, x].Green;
        //            double r = (1 - a) * img[y, x].Red;
        //            if (a != 0)
        //                Debug.Write(" a:" + a.ToString("0.00") + ",");
        //            img[y, x] = new Bgr(b, g, r);
        //        }
        //        Debug.WriteLine("");
        //    }
        //}

        //private void AddMask(Image<Bgr, byte> img, Bitmap mask)
        //{
        //    //BitmapData bitmapData = mask.LockBits(new Rectangle(0, 0, mask.Width, mask.Height), ImageLockMode.ReadOnly, mask.PixelFormat);
        //    //IntPtr ptr = bitmapData.Scan0;
        //    //unsafe
        //    //{
        //    //    byte* p = (byte*)ptr;
        //    //    for (int y = 0; y < mask.Height; y++)
        //    //    {
        //    //        for (int x = 0; x < mask.Width; x++)
        //    //        {
        //    //            int index = y * mask.Width * 4 + x * 4;
        //    //            double a = p[index + 3] / 255;
        //    //            double b = (1 - a) * img[y, x].Blue;
        //    //            double g = (1 - a) * img[y, x].Green;
        //    //            double r = (1 - a) * img[y, x].Red;
        //    //            img[y, x] = new Bgr(b, g, r);
        //    //        }
        //    //    }
        //    //}
        //    //mask.UnlockBits(bitmapData);
            

        //    //for (int y = 0; y < img.Height; y++)
        //    //{
        //    //    for (int x = 0; x < img.Width; x++)
        //    //    {
        //    //        double a = mask.GetPixel(x, y).A / 255;
        //    //        double b = (1 - a) * img[y, x].Blue;
        //    //        double g = (1 - a) * img[y, x].Green;
        //    //        double r = (1 - a) * img[y, x].Red;
        //    //        //if (a != 0)
        //    //        //    Debug.Write(" a:" + a.ToString("0.00") + ",");
        //    //        img[y, x] = new Bgr(b, g, r);
        //    //    }
        //    //    Debug.WriteLine("");
        //    //}
        //}

        //private void AddFire(Image<Bgr, byte> img, Image<Bgra, byte> fire, float alpha)
        //{
        //    MIplImage imgIpl = img.MIplImage;
        //    MIplImage fireIpl = fire.MIplImage;
        //    float imgAlpha = 1 - alpha;
        //    unsafe
        //    {
        //        byte* imgPixel = (byte*)imgIpl.imageData;
        //        byte* firePixel = (byte*)fireIpl.imageData;
        //        for (int h = 0; h < img.Height; h++)
        //        {
        //            for (int w = 0; w < img.Width; w++)
        //            {
        //                int point = w * 3;
        //                if (firePixel[point] != DEFALUT_TRANSPARENT_COLOR_B ||
        //                    firePixel[point + 1] != DEFALUT_TRANSPARENT_COLOR_G ||
        //                    firePixel[point + 2] != DEFALUT_TRANSPARENT_COLOR_R)
        //                {
        //                    imgPixel[point] = Convert.ToByte(imgPixel[point] * imgAlpha + firePixel[point] * alpha);
        //                    imgPixel[point + 1] = Convert.ToByte(imgPixel[point + 1] * imgAlpha + firePixel[point + 1] * alpha);
        //                    imgPixel[point + 2] = Convert.ToByte(imgPixel[point + 2] * imgAlpha + firePixel[point + 2] * alpha);
        //                }
        //            }
        //            firePixel = firePixel + fireIpl.widthStep;
        //            imgPixel = imgPixel + imgIpl.widthStep;
        //        }
        //    }
        //}

        //private void AddFire(Image<Bgr, byte> img, Image<Bgra, byte> fire)
        //{
        //    MIplImage imgIpl = img.MIplImage;
        //    MIplImage fireIpl = fire.MIplImage;
        //    unsafe
        //    {
        //        byte* imgPixel = (byte*)imgIpl.imageData;
        //        byte* firePixel = (byte*)fireIpl.imageData;
        //        for (int h = 0; h < img.Height; h++)
        //        {
        //            for (int w = 0; w < img.Width; w++)
        //            {
        //                int point = w * 4;
        //                double alpha = firePixel[point + 3] / 255;
        //                imgPixel[point] = Convert.ToByte(imgPixel[point] * (1 - alpha) + firePixel[point] * alpha);
        //                imgPixel[point + 1] = Convert.ToByte(imgPixel[point + 1] * (1 - alpha) + firePixel[point + 1] * alpha);
        //                imgPixel[point + 2] = Convert.ToByte(imgPixel[point + 2] * (1 - alpha) + firePixel[point + 2] * alpha);
        //            }
        //            firePixel = firePixel + fireIpl.widthStep;
        //            imgPixel = imgPixel + imgIpl.widthStep;
        //        }
        //    }
        //}

        //private Image<Bgr, byte> LieDown(Image<Bgr, byte> src, float ratio)
        //{
        //    int width = src.Width;
        //    int height = src.Height;

        //    float x1 = 0 + width / (1 / LIEDOWN_WIDTH_SPACE) * ratio;
        //    float x2 = width - width / (1 / LIEDOWN_WIDTH_SPACE) * ratio;
        //    float y = 0 + height / (1 / LIEDOWN_HEIGHT_SPACE) * ratio;

        //    PointF[] srcPoint = new PointF[4];
        //    srcPoint[0] = new PointF(0, 0);
        //    srcPoint[1] = new PointF(width - 1, 0);
        //    srcPoint[2] = new PointF(0, height - 1);
        //    srcPoint[3] = new PointF(width - 1, height - 1);

        //    PointF[] dstPoint = new PointF[4];
        //    dstPoint[0] = new PointF(x1 , y);
        //    dstPoint[1] = new PointF(x2, y);
        //    dstPoint[2] = new PointF(0, height);
        //    dstPoint[3] = new PointF(width, height);

        //    HomographyMatrix mywarpmat = CameraCalibration.GetPerspectiveTransform(srcPoint, dstPoint);
        //    return src.WarpPerspective(mywarpmat, Emgu.CV.CvEnum.INTER.CV_INTER_NN, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new Bgr(0, 0, 0));
        //}

        //private Image<Bgr, Byte> CreateImage(int b, int g, int r)
        //{
        //    Image<Bgr, Byte> img = new Image<Bgr, byte>(_frameWidth, _frameHeight);
        //    // 透過直接操作記憶體加速
        //    MIplImage ipl = img.MIplImage;
        //    unsafe
        //    {
        //        byte* npixel = (byte*)ipl.imageData;
        //        for (int h = 0; h < _frameHeight; h++)
        //        {
        //            for (int w = 0; w < _frameWidth; w++)
        //            {
        //                int point = w * 3;
        //                npixel[point] = (byte)b;
        //                npixel[point + 1] = (byte)g;
        //                npixel[point + 2] = (byte)r;
        //            }
        //            npixel = npixel + ipl.widthStep;
        //        }
        //    }
        //    return img;
        //}
    }
}
