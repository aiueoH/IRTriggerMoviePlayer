using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DirectionRecognition;

namespace Furnace
{
    public class Model
    {
        public delegate void OnFrameUpdateHandler(object sender, EventArgs e);
        public event OnFrameUpdateHandler OnFrameUpdate;

        private Camera _camera;
        private Player _player;
        private BlockingDetector _blockingDetector;
        private GateController _gateController;

        private bool _isExecuting;
        private readonly Object _isExecutingToken = new Object();
        private int _BEFORE_PLAY_TIME;

        private bool IsExecuting
        {
            set
            {
                lock (_isExecutingToken)
                {
                    _isExecuting = value;
                }
            }
        }

        private bool TestAndSetIsExecuting
        {
            get
            {
                lock (_isExecutingToken)
                {
                    if (!_isExecuting)
                    {
                        _isExecuting = true;
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        public Model()
        {
            _isExecuting = false;

            Dictionary<string, string> config = new Dictionary<string, string>();
            using (StreamReader file = new StreamReader(@"./config.txt"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line == "" || line[0] == '#')
                        continue;
                    string[] field = line.Split('=');
                    config.Add(field[0], field[1]);
                }
            }

            _BEFORE_PLAY_TIME = Convert.ToInt32(config["BEFORE_PLAY_TIME"]);

            BinaryHistogram.BinaryHistogramSetting bhSetting = new BinaryHistogram.BinaryHistogramSetting();
            bhSetting.LeftBottomROI = new Rectangle(
                Convert.ToInt32(config["LBROI_X"]), 
                Convert.ToInt32(config["LBROI_Y"]), 
                Convert.ToInt32(config["LBROI_W"]), 
                Convert.ToInt32(config["LBROI_H"]));
            bhSetting.LeftTopROI = new Rectangle(
                Convert.ToInt32(config["LTROI_X"]), 
                Convert.ToInt32(config["LTROI_Y"]), 
                Convert.ToInt32(config["LTROI_W"]), 
                Convert.ToInt32(config["LTROI_H"]));
            bhSetting.RightBottomROI = new Rectangle(
                Convert.ToInt32(config["RBROI_X"]), 
                Convert.ToInt32(config["RBROI_Y"]), 
                Convert.ToInt32(config["RBROI_W"]), 
                Convert.ToInt32(config["RBROI_H"]));
            bhSetting.RightTopROI = new Rectangle(
                Convert.ToInt32(config["RTROI_X"]), 
                Convert.ToInt32(config["RTROI_Y"]), 
                Convert.ToInt32(config["RTROI_W"]), 
                Convert.ToInt32(config["RTROI_H"]));
            bhSetting.HueMinA = Convert.ToDouble(config["HUEMINA"]);
            bhSetting.HueMaxA = Convert.ToDouble(config["HUEMAXA"]);
            bhSetting.HueMinB = Convert.ToDouble(config["HUEMINB"]);
            bhSetting.HueMaxB = Convert.ToDouble(config["HUEMAXB"]);
            bhSetting.ED = Convert.ToInt32(config["ED"]);
            _camera = new Camera(
                Convert.ToInt32(config["CUT_LEFT_UP_X"]), 
                Convert.ToInt32(config["CUT_LEFT_UP_Y"]), 
                Convert.ToInt32(config["CUT_RIGHT_DOWN_X"]), 
                Convert.ToInt32(config["CUT_RIGHT_DOWN_Y"]), 
                Convert.ToDouble(config["SCALE"]), 
                Convert.ToDouble(config["ROTATE"]), 
                Convert.ToBoolean(config["SAVE_PICTURE"]), 
                bhSetting);
            _player = new Player(
                Convert.ToInt32(config["PRE_FRAME_TIME"]), 
                Convert.ToInt32(config["BURN_FRAME_TIME"]), 
                Convert.ToInt32(config["PREFRAME_COUNT"]), 
                Convert.ToInt32(config["BURNFRAME_COUNT"]), 
                Convert.ToBoolean(config["USE_MASK"]));
            _player.OnFrameUpdate += PlayerOnFrameUpdateHandler;
            _player.OnPlayerEnd += OnPlayerEndHandler;

            _gateController = new GateController();
            _blockingDetector = new BlockingDetector(Convert.ToInt32(config["BLOCKING_COUNT"]));
            _blockingDetector.OnBlockingDetect += OnBlockingDetectHandler;
        }

        public void Close()
        {
            try
            {
                _camera.Close();
                _player.Close();
                _gateController.Close();
                _blockingDetector.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public Image<Bgra, Byte> GetFrame()
        {
            return _player.GetFrame();
        }

        public void Execute()
        {
            if (TestAndSetIsExecuting)
            {
                _gateController.OpenLight();
                Thread.Sleep(1000);
                Thread.Sleep(_BEFORE_PLAY_TIME);
                Image<Bgr, byte> goldPaper = _camera.GetPhoto();
                _gateController.CloseLight();
                _gateController.OpenGate();
                _player.SetGoldPaper(goldPaper);
                _player.Play();
            }
        }

        private void EndOfExecute()
        {
            _gateController.CloseGate();
            IsExecuting = false;
        }

        private void PlayerOnFrameUpdateHandler(object sender, EventArgs e)
        {
            if (OnFrameUpdate != null)
                OnFrameUpdate(this, null);
        }

        private void OnPlayerEndHandler(object sender, EventArgs e)
        {
            EndOfExecute();
        }

        private void OnBlockingDetectHandler(object sender, EventArgs e)
        {
            Execute();
        }

        public void PlayPreAni()
        {
            _player.PlayPreAni();
        }
    }
}
