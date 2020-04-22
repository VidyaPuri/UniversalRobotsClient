using Caliburn.Micro;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public BluetoothConnection(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
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
                SerialPort spl = (SerialPort)sender;
                Debug.WriteLine($"Data {spl.ReadLine()} \n");
                _eventAggregator.BeginPublishOnUIThread(spl.ReadLine());
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
            _eventAggregator.BeginPublishOnUIThread(text); // < ------------------------------------------------------------------------------------------------------------- to je sam testno
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
