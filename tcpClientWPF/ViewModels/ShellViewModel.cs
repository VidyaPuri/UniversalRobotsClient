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
using System.Globalization;

namespace tcpClientWPF.ViewModels
{
    public class ShellViewModel : Screen
    {
        #region Private Members

        private Socket _socket;
        private byte[] _buffer;
        private int _Port = 30003;
        private string _IpAddress = "192.168.58.101";
        private double[] _RobotPose = { 0, 0, 0, 0, 0, 0 };
        private bool _ConnectionStatus = false;

        private bool _io0;
        private bool _io1;
        private bool _io2;
        private bool _io3;
        private bool _io4;
        private bool _io5;
        private bool _io6;
        private bool _io7;

        #endregion 

        #region Constructor

        public ShellViewModel()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectionStatus = _socket.Connected;
        }

        #endregion

        #region Properties Initialisation

        /// <summary>
        /// Script (send command) Initialisation
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// IpAddress Initialisation
        /// </summary>
        public string IpAddress
        {
            get { return _IpAddress; }
            set => Set(ref _IpAddress, value); 
        }

        /// <summary>
        /// Port Initialisation
        /// </summary>
        public int Port
        {
            get => _Port;
            set => Set(ref _Port, value);
        }

        /// <summary>
        /// RobotPose Initialisation
        /// </summary>
        public double[] RobotPose
        {
            get => _RobotPose;
            set => Set(ref _RobotPose, value);
        }

        public bool ConnectionStatus
        {
            get { return _ConnectionStatus; }
            set => Set(ref _ConnectionStatus, value);
        }

        #region I/O properties

        /// <summary>
        /// Digital I/O 0
        /// </summary>
        public bool Io0
        {
            get => _io0;
            set
            {
                _io0 = value;
                NotifyOfPropertyChange(() => Io0);
                SendIO(0, value);
            }
        }

        /// <summary>
        /// Digital I/O 1
        /// </summary>
        public bool Io1
        {
            get => _io1;
            set
            {
                _io1 = value;
                NotifyOfPropertyChange(() => Io1);
                SendIO(1, value);
            }
        }

        /// <summary>
        /// Digital I/O 2
        /// </summary>
        public bool Io2
        {
            get => _io2;
            set
            {
                _io2 = value;
                NotifyOfPropertyChange(() => Io2);
                SendIO(2, value);
            }
        }

        /// <summary>
        /// Digital I/O 3
        /// </summary>
        public bool Io3
        {
            get => _io3;
            set
            {
                _io3 = value;
                NotifyOfPropertyChange(() => Io3);
                SendIO(3, value);
            }
        }

        /// <summary>
        /// Digital I/O 4
        /// </summary>
        public bool Io4
        {
            get => _io4;
            set
            {
                _io4 = value;
                NotifyOfPropertyChange(() => Io4);
                SendIO(4, value);
            }
        }

        /// <summary>
        /// Digital I/O 5
        /// </summary>
        public bool Io5
        {
            get => _io5;
            set
            {
                _io5 = value;
                NotifyOfPropertyChange(() => Io5);
                SendIO(5, value);
            }
        }

        /// <summary>
        /// Digital I/O 5
        /// </summary>
        public bool Io6
        {
            get => _io6;
            set
            {
                _io6 = value;
                NotifyOfPropertyChange(() => Io6);
                SendIO(6, value);
            }
        }

        /// <summary>
        /// Digital I/O 5
        /// </summary>
        public bool Io7
        {
            get => _io7;
            set
            {
                _io7 = value;
                NotifyOfPropertyChange(() => Io7);
                SendIO(7, value);
            }
        }

        #endregion

        #endregion

        #region Socket Methods

        /// <summary>
        /// ConnectToRobot Button Method
        /// </summary>
        public void ConnectToRobot()
        {
            Debug.WriteLine($"IpAddress: { IpAddress}");
            Debug.WriteLine($"Port: { Port}");

            Task.Run(() =>
            {
                Connect("192.168.56.101", 30003);
            });
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

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                RobotPose = new double[] { 0, 0, 0, 0, 0, 0 };
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ConnectionStatus = _socket.Connected;
            }
            catch (SocketException exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Connect Callback
        /// </summary>
        /// <param name="result"></param>
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                if (_socket.Connected)
                {
                    ConnectionStatus = _socket.Connected;
                    StartReceiving();
                }
                else
                    Console.WriteLine("Could not connect!");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

                double[] rp = PacketHandler.Handle(packet, packet.Length);

                UpdateUI(rp);
                StartReceiving();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
        /// Update UI Values
        /// </summary>
        /// <param name="robotPose"></param>
        public void UpdateUI(double[] robotPose)
        {
            RobotPose = robotPose;
        }



        /// <summary>
        /// Send string command
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        private static void Send(Socket client, String data)
        {
            try
            {
                var stringData = data + "\n";
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.ASCII.GetBytes(stringData);

                // Begin sending the data to the remote device.  

                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
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

                // Signal that all bytes have been sent.  
                //sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Sending digital IO on/off
        /// </summary>
        /// <param name="io"></param>
        /// <param name="value"></param>
        private void SendIO(int io, bool value)
        {
            try
            {
                string data = $"set_digital_out({io},{value})";
                Send(_socket, data);
            }
            catch(SocketException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void SendScript()
        {
            Send(_socket, Script);
        }

        #endregion

        #region Move Methods

        public void J0Add()
        {
            try
            {
                double x = RobotPose[0];
                x += 0.02;

                string msg = $"movej([" +
                $"{x.ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[1].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[2].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[3].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[4].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[5].ToString(new CultureInfo("en-US"))}]," +
                $" a = 2, v = 1)";

                //msg = "movej([0.540518270502519, -2.35033018411227, -1.31663103726659, -2.277573660445824, 3.352832342366564, -1.229196745489491], a = 2, v = 1)";
                Send(_socket, msg);

            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void J0Sub()
        {
            try
            {
                double x = RobotPose[0];
                x -= 0.02;

                string msg = $"movej([" +
                $"{x.ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[1].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[2].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[3].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[4].ToString(new CultureInfo("en-US"))}, " +
                $"{RobotPose[5].ToString(new CultureInfo("en-US"))}]," +
                $" a = 2, v = 1)";

                //msg = "movej([0.540518270502519, -2.35033018411227, -1.31663103726659, -2.277573660445824, 3.352832342366564, -1.229196745489491], a = 2, v = 1)";
                Send(_socket, msg);

            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

    }

}
