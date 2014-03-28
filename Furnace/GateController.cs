/*
 * 
 * 控制伺服馬達閘門兼LED燈光
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;

namespace Furnace
{
    public class GateController
    {
        private const string PORTNAME = "COM3";
        private const int BAUDRATE = 9600;
        private const string OPEN_COMMAND = "0";
        private const string CLOSE_COMMAND = "1";
        private const string OPEN_LIGHT = "2";
        private const string CLOSE_LIGHT = "3";

        private SerialPort _serialPort;

        private delegate void WorkFunction();
        private ConcurrentQueue<WorkFunction> _workFuntionQueue;

        private const int EXECUTE_INTERVAL = 33;
        private Thread _executerThread;

        public GateController()
        {
            _workFuntionQueue = new ConcurrentQueue<WorkFunction>();

            _serialPort = new SerialPort();
            _serialPort.PortName = PORTNAME;
            _serialPort.BaudRate = BAUDRATE;
            _serialPort.Open();

            //_executerThread = new Thread(new ThreadStart(Executer));
            //_executerThread.Start();
        }

        public void Close()
        {
            //_executerThread.Abort();
            _serialPort.Close();
            _serialPort.Dispose();
        }

        private void Executer()
        {
            while (true)
            {
                Thread.Sleep(EXECUTE_INTERVAL);
                WorkFunction wf;
                if (_workFuntionQueue.TryDequeue(out wf))
                {
                    wf();
                }
            }
        }

        //public void OpenLight()
        //{
        //    _workFuntionQueue.Enqueue(OpenLightImp);
        //}

        //public void CloseLight()
        //{
        //    _workFuntionQueue.Enqueue(CloseLightImp);
        //}

        //public void OpenGate()
        //{
        //    _workFuntionQueue.Enqueue(OpenGateImp);
        //}

        //public void CloseGate()
        //{

        //    _workFuntionQueue.Enqueue(CloseGateImp);
        //}

        public void OpenLight()
        {
            OpenLightImp();
        }

        public void CloseLight()
        {
            CloseLightImp();
        }

        public void OpenGate()
        {
            OpenGateImp();
        }

        public void CloseGate()
        {

            CloseGateImp();
        }

        private void OpenGateImp()
        {
            _serialPort.Write(OPEN_COMMAND);
            Thread.Sleep(2000);
        }

        private void CloseGateImp()
        {
            _serialPort.Write(CLOSE_COMMAND);
            Thread.Sleep(2000);
        }

        private void OpenLightImp()
        {
            _serialPort.Write(OPEN_LIGHT);
            
            Thread.Sleep(100);
        }

        private void CloseLightImp()
        {
            _serialPort.Write(CLOSE_LIGHT);
            Thread.Sleep(100);
        }
    }
}
