using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using AsynchronousSocketClient.Networking;
using System.Windows;
using System.Threading;
using AsynchronousSockeClient.Networking;

namespace tcpClientWPF.ViewModels
{
    public class ShellViewModel : Screen
    {
        #region Private Members

        private Socket _socket;
        private byte[] _buffer;

        #endregion 

        #region Constructor

        public ShellViewModel()
        {
            //StartClient();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #endregion

        #region Properties Initialisation

        private string _IpAddress = "192.168.58.101";

        public string Script { get; set; }

        public string IpAddress
        {
            get { return _IpAddress; }
            set => Set(ref _IpAddress, value); 
        }

        private int _Port = 30003;

        public int Port
        {
            get => _Port;
            set => Set(ref _Port, value);
        }

        private double[] _RobotPose = { 0, 0, 0, 0, 0, 0 };   

        public double[] RobotPose
        {
            get => _RobotPose;
            set => Set(ref _RobotPose, value);
        }

        private bool _io0;
        public bool io0
        {
            get => _io0;
            set
            {
                _io0 = value;
                NotifyOfPropertyChange(() => io0);
                SendIO(0, value);
            }
        }


        #endregion

        #region Socket Methods

        public void ConnectToRobot()
        {
            Debug.WriteLine($"IpAddress: { IpAddress}");
            Debug.WriteLine($"Port: { Port}");

            Task.Run(() =>
            {
                Connect("192.168.56.101", 30003);
            });
        }

        public void Connect(string ipAddress, int port)
        {
            try
            {
                if (!_socket.Connected)
                {
                    _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectCallback, null);
                    Console.WriteLine("Connected to the server!");
                }
                else
                {
                    Debug.WriteLine("Already connected to the server!");
                }
            } catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                if (_socket.Connected)
                    StartReceiving();
                else
                    Console.WriteLine("Could not connect!");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

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

                double[] rp = PacketHandler.Handle(packet, packet.Length);

                UpdateUI(rp);
                StartReceiving();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void StartReceiving()
        {
            try
            {
                _buffer = new byte[1116];
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceivedCallback, null);
            }
            catch { }
        }

        public void UpdateUI(double[] robotPose)
        {
            RobotPose = robotPose;
        }

        public void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            RobotPose = new double[]{ 0, 0, 0, 0, 0, 0 };
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private static void Send(Socket client, String data)
        {
            var stringData = data + "\n";
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(stringData);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                //sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendIO(int io, bool value)
        {
            string data = $"set_digital_out({io},{value})";
            Send(_socket, data);
        }

        public void SendScript()
        {
            Send(_socket, Script);
        }

        #endregion
    }

}
