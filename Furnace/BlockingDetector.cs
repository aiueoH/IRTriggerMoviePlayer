using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;

namespace Furnace
{
    public class BlockingDetector
    {
        private const string PORTNAME = "COM4";
        private const int BAUDRATE = 9600;
        private const int DETECT_DURATION = 3000;
        private const int DETECT_INTERVAL = 33;
        private const char IS_BLOCKING_SIGNAL = '0';

        public delegate void OnBlockingDetectHandler(object sender, EventArgs e);
        public event OnBlockingDetectHandler OnBlockingDetect;

        private SerialPort _serialPort;
        private ConcurrentStack<bool> _blockingRecords;
        private Thread _blockingCheckerThread;

        private int _BLOCKING_COUNT;
        private int _blockingCount;


        public BlockingDetector(int BLOCKING_COUNT)
        {
            _BLOCKING_COUNT = BLOCKING_COUNT;

            _blockingRecords = new ConcurrentStack<bool>();
            //_blockingCheckerThread = new Thread(BlockingChecker);
            //_blockingCheckerThread.Start();
            
            _serialPort = new SerialPort();
            _serialPort.PortName = PORTNAME;
            _serialPort.BaudRate = BAUDRATE;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            _serialPort.Open();
        }

        public void Close()
        {
            //_blockingCheckerThread.Abort();
            _serialPort.Close();
            _serialPort.Dispose();
        }

        /// <summary>
        /// SerialPort 接收到 Data 的事件處理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = sender as SerialPort;
            if (sp != null)
            {
                string data = sp.ReadExisting();

                //////////////////////////////////////////////////////

                //Debug.WriteLine("IR Data received. => " + data);
                //foreach (Char c in data.ToCharArray())
                //{
                //    bool b = (c == IS_BLOCKING_SIGNAL) ? true : false;
                //    _blockingRecords.Push(b);
                //}

                //////////////////////////////////////////////////////

                foreach (Char c in data.ToCharArray())
                {
                    if (c == IS_BLOCKING_SIGNAL)
                        _blockingCount++;
                    else
                        _blockingCount = 0;
                    if (_blockingCount > _BLOCKING_COUNT)
                    {
                        if (OnBlockingDetect != null)
                            OnBlockingDetect(this, null);
                    }
                }

            }

        }

        /// <summary>
        /// 不斷的檢查是否有持續性遮斷
        /// </summary>
        private void BlockingChecker()
        {
            while (true)
            {
                BlockingCheckerImp();
            }
        }

        /// <summary>
        /// 檢查是否有持續性遮斷實作
        /// </summary>
        private void BlockingCheckerImp()
        {
            Thread.Sleep(DETECT_INTERVAL);
            bool isBeginning;
            lock (_blockingRecords)
            {
                isBeginning = CheckBlockingBegin(_blockingRecords);
                _blockingRecords.Clear();
            }
            if (isBeginning && CheckBlockingContinueDuration(_blockingRecords))
            {
                if (OnBlockingDetect != null)
                {
                    OnBlockingDetect(this, null);
                    _blockingRecords.Clear();
                }
            }
        }

        /// <summary>
        /// 判斷是否開始遮斷
        /// </summary>
        /// <param name="blockingrecords"></param>
        /// <returns></returns>
        private bool CheckBlockingBegin(ConcurrentStack<bool> blockingrecords)
        {
            bool isBlockingLatest;
            return (blockingrecords.TryPop(out isBlockingLatest) && isBlockingLatest) ? true : false;
        }

        /// <summary>
        /// 以一段時間內 BlockingRecords 是否一直為空，判斷持否為持續性遮斷
        /// </summary>
        /// <param name="blockingrecords"></param>
        /// <returns></returns>
        private bool CheckBlockingContinueDuration(ConcurrentStack<bool> blockingrecords)
        {
            Thread.Sleep(DETECT_DURATION);
            return blockingrecords.IsEmpty ? true : false;
        }

    }
}
