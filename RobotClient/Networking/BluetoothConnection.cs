using Caliburn.Micro;
using RobotInterface.Models;
using System;
using System.Diagnostics;
using System.IO.Ports;

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
        public void Connect()
        {
            serial.PortName = "COM4";
            serial.BaudRate = 38400;

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
                try
                {
                    serial.Write(text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

    }
}
