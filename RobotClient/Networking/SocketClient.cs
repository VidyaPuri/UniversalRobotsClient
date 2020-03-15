using AsynchronousSockeClient.Networking;
using Caliburn.Micro;
using RobotClient.Models;
using RobotClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RobotClient.Networking
{
    public class SocketClient
    {

        private Socket _socket;
        private byte[] _buffer;
        public IEventAggregator _eventAggregator { get; }
        private ConnectionStatusModel connectionStatus = new ConnectionStatusModel();
        private RobotOutputPackage robotOutputPackage = new RobotOutputPackage();

        public SocketClient(IEventAggregator eventAggregator)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Async Connect Method
        /// </summary>
        /// <param name="ipAddress">Input IP address</param>
        /// <param name="port">Port</param>
        public void Connect(string ipAddress, int port)
        {
            try
            {
                IPAddress ipa = IPAddress.Parse(ipAddress);
                if (!_socket.Connected)
                {
                    connectionStatus.ConnectionStatusStr = "Connecting";
                    _socket.BeginConnect(new IPEndPoint(ipa, port), ConnectCallback, null);
                    connectionStatus.CanConnect = false;
                    PublishEventToUI();
                }
                else
                {
                    Debug.WriteLine("Already connected to the server!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connection exception message: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (SocketException exception)
            {
                Debug.WriteLine($"Disconnect exception message: {exception.Message}");
            }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            robotOutputPackage.RobotJoints = new double[] { 0, 0, 0, 0, 0, 0 };
            robotOutputPackage.RobotPose = new double[] { 0, 0, 0, 0, 0, 0 };

            connectionStatus.ConnectionStatusBool = _socket.Connected;
            connectionStatus.ConnectionStatusStr = "Stopped";
            connectionStatus.ConnectToggle = "Connect";
            connectionStatus.CanConnect = true;
            PublishEventToUI();
        }

        /// <summary>
        /// Connect Callback
        /// </summary>
        /// <param name="result"></param>
        private void ConnectCallback(IAsyncResult result)
        {
            Console.WriteLine($"Status: {_socket.Connected.ToString()}");
            connectionStatus.ConnectionStatusStr = "Running";
            connectionStatus.CanConnect = true;
            PublishEventToUI();

            // If can not connect, call disconnect to reinitialize the socket
            if (!_socket.Connected)
                Disconnect();

            try
            {
                if (_socket.Connected)
                {
                    connectionStatus.ConnectToggle = "Disconnect";
                    connectionStatus.ConnectionStatusBool = _socket.Connected;
                    PublishEventToUI();

                    StartReceiving();
                }
                else
                    Console.WriteLine("Could not connect!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connect callback exception message: {ex.Message}");
            }
        }

        /// <summary>
        /// ReceivedCallback when we receive something from Socket
        /// </summary>
        /// <param name="result"></param>
        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {
                int buffLength = _socket.EndReceive(result);
                byte[] packet = new byte[buffLength];

                Array.Copy(_buffer, packet, packet.Length);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(packet);

                // Handle packet
                robotOutputPackage = PacketHandler.Handle(packet, packet.Length);

                // Update UI
                PublishEventToUI();

                // Start receiving next package
                StartReceiving();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Received calback exception message: {ex.Message}");
            }
        }

        /// <summary>
        /// Start Receiving
        /// </summary>
        public void StartReceiving()
        {
            try
            {
                _buffer = new byte[1116];
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceivedCallback, null);
            }
            catch { }
        }

        /// <summary>
        /// Send string command
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        public void Send(Socket client, String data)
        {
            try
            {
                var stringData = data + "\n";
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.ASCII.GetBytes(stringData);

                // Begin sending the data to the remote device.  
                //client = _socket;
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"Send exception message: {ex.Message}");
            }
        }

        /// <summary>
        /// Callback after command is sent
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Send callback exception message : {e.Message}");
            }
        }

        /// <summary>
        /// Sending digital IO on/off
        /// </summary>
        /// <param name="io"></param>
        /// <param name="value"></param>
        public void SendIO(int io, bool value)
        {
            try
            {
                string data = $"set_digital_out({io},{value})";
                Send(_socket, data);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"SendIO exception message: {ex.Message}");
            }
        }

        #region Helper Methods

        private void PublishEventToUI()
        {
            _eventAggregator.BeginPublishOnUIThread(connectionStatus);
            _eventAggregator.BeginPublishOnUIThread(robotOutputPackage);
            _eventAggregator.BeginPublishOnUIThread(_socket);
        }

        #endregion
    }
}
