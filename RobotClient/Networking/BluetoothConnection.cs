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
        public bool SerialStatus { get; set; }
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
            serial.PortName = comPort;
            serial.BaudRate = Int32.Parse(baudRate);

            // Sets the Serial Status 
            SerialStatus = serial.IsOpen;

            try
            {
                if (!serial.IsOpen)
                {
                    serial.Open();
                    SerialStatus = serial.IsOpen;
                    _eventAggregator.BeginPublishOnUIThread(SerialStatus);
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
            if (serial.IsOpen)
            {
                serial.Close();
                SerialStatus = serial.IsOpen;
                _eventAggregator.BeginPublishOnUIThread(SerialStatus);
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
                Task.Run(() =>
                {
                    try
                    {
                        serial.WriteLine(text);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
        }

        /// <summary>
        /// Returns the list of baud rates
        /// </summary>
        /// <returns></returns>
        public string[] GetBaudRates()
        {
            string[] output = {"300", "1200","2400","4800", "9600","19200", "38400", "57600","1000000"};

            return output;
        }

    }
}
