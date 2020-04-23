using System;
using System.IO.Ports;
using System.Diagnostics;

namespace RobotInterface.Networking
{
    public class SerialCommunication
    {

        SerialPort port;

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
                port.Open();
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
            if(port.IsOpen)
                port.Close();
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
