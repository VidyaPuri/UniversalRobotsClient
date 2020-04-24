using System;
using System.IO.Ports;
using System.Diagnostics;
using Caliburn.Micro;
using RobotInterface.Models;

namespace RobotInterface.Networking
{
    public class SerialCommunication
    {

        private SerialPort port;
        private SerialStatusModel serialStatus = new SerialStatusModel();
        public IEventAggregator _eventAggregator { get; }

        public SerialCommunication(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
        /// <summary>
        /// Open new serial port
        /// </summary>
        public void OpenSerialPort()
        {
            port = new SerialPort
            {
                PortName = "COM3",
                BaudRate = 9600
            };

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                    serialStatus.USBSerialStatus = port.IsOpen;
                    _eventAggregator.BeginPublishOnUIThread(serialStatus);
                }
            }
              catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Close the port
        /// </summary>
        public void CloseSerialPort()
        {
            if (port.IsOpen)
            {
                port.Close();
                serialStatus.USBSerialStatus = port.IsOpen;
                _eventAggregator.BeginPublishOnUIThread(serialStatus);
            }
        }

        /// <summary>
        /// Send value to port method
        /// </summary>
        /// <param name="value"></param>
        public void SendToPort(double value)
        {
            if(port != null)
            {
                if (port.IsOpen)
                {
                    port.WriteLine(value.ToString());
                }
            }
        }
    }
}
