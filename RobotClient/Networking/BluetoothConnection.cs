using Caliburn.Micro;
using RobotInterface.Models;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;

namespace RobotInterface.Networking
{
    public class BluetoothConnection
    {
        readonly SerialPort serial = new SerialPort();
        public IEventAggregator _eventAggregator { get; }
        private SerialStatusModel serialStatus = new SerialStatusModel();
        private LogModel logModel = new LogModel();
        private int idx;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public BluetoothConnection(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            idx = 0;
        }

        /// <summary>
        /// Connect BT
        /// </summary>
        public void Connect(string comPort, string baudRate)
        {
            // Sets the Serial Status 
            serialStatus.BTSerialStatus = serial.IsOpen;

            try
            {
                if (!serial.IsOpen)
                {
                    serial.RtsEnable = true;
                    serial.PortName = comPort;
                    serial.BaudRate = Int32.Parse(baudRate);
                    serial.Open();
                    serialStatus.BTSerialStatus = serial.IsOpen;
                    serialStatus.ComType = "BT";
                    _eventAggregator.BeginPublishOnUIThread(serialStatus);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // DataReceived event handler
            serial.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
        }

        /// <summary>
        /// Disconnect BT
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (serial.IsOpen)
                {
                    serial.Close();
                    serialStatus.BTSerialStatus = serial.IsOpen;
                    serialStatus.ComType = "BT";
                    _eventAggregator.BeginPublishOnUIThread(serialStatus);
                }
            }
            catch ( Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

        /// <summary>
        /// Received data from Arduino
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                idx++;
                SerialPort spl = (SerialPort)sender;
                //string received = spl.ReadLine();
                logModel.Message = spl.ReadLine();
                logModel.Timestamp = DateTime.Now.ToString("HH:mm:ss");
                logModel.Idx = idx;

                Debug.WriteLine($"Data {spl.ReadLine()} \n");
                Debug.WriteLine($"Received {logModel.Message}");
                _eventAggregator.BeginPublishOnUIThread(logModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Send string to arduino via BT
        /// </summary>
        public void SendString(string text)
        {
            if (serial.IsOpen)
            {
                Task.Run(() =>
                {
                    try
                    {
                        serial.Write(text);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
        }

        /// <summary>
        /// Send string to arduino via BT
        /// </summary>
        public void SendStringLine(string text)
        {
            if (serial.IsOpen)
            {
                Task.Run( () =>
                {
                    try
                    {
                        serial.WriteLine(text);
                        Debug.WriteLine(text);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
        }
    }
}
