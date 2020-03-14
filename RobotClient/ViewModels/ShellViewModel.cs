using Caliburn.Micro;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using AsynchronousSockeClient.Networking;
using System.Globalization;
using RobotClient.Models;
using System.Windows;
using SharpDX.XInput;
using System.Threading;

namespace RobotClient.ViewModels
{
    public class ShellViewModel : Screen
    {

        #region Window Control

        private WindowState windowState;
        public WindowState WindowState
        {
            get { return windowState; }
            set
            {
                windowState = value;
                NotifyOfPropertyChange(() => WindowState);
            }
        }

        public void MaximizeWindow()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        public void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
        }

        public bool myCondition { get { return (false); } }

        public void CloseWindow()
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Private Members

        private Socket _socket;
        private byte[] _buffer;
        private int _Port = 30003;
        private string _IpAddress = "192.168.56.101";

        private bool _ControllerConnectionStatusBool;
        private bool _ConnectionStatusBool = false;
        private string _ConnectionStatusStr = "Disconnected";
        private string _ConnectToggle = "Connect";
        private bool _CanConnect;

        private double _TranslationRate = 0.01;
        private double _RotationRate = 0.01;
        private bool startButtonPressed = false;

        private readonly Controller _controller = new Controller(UserIndex.One);
        private readonly int RefreshRate = 60;
        private  Timer _timer;

        private RobotOutputPackage _RobotOutputPackage = new RobotOutputPackage();
        private double[] _RobotJoints = { 0, 0, 0, 0, 0, 0 };
        private double[] _RobotPose = { 0, 0, 0, 0, 0, 0 };

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
            ConnectionStatusBool = _socket.Connected;
            CanConnect = true;
            _ControllerMoveToggle = "TCP";

            _timer = new Timer(obj => ControllerUpdate());
            StartController();
        }

        #endregion

        #region Properties Initialisation

        #region Robot Output Package

        /// <summary>
        /// Robot output package initialisation
        /// </summary>
        public RobotOutputPackage RobotOutputPackage
        {
            get { return _RobotOutputPackage; }
            set => Set(ref _RobotOutputPackage, value);
        }

        /// <summary>
        /// Robot pose initalisation
        /// </summary>
        public double[] RobotPose
        {
            get { return _RobotPose; }
            set => Set(ref _RobotPose, value);
        }

        /// <summary>
        /// RobotPose Initialisation
        /// </summary>
        public double[] RobotJoints
        {
            get => _RobotJoints;
            set => Set(ref _RobotJoints, value);
        }

        #endregion

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
        /// Connection status initialisation
        /// </summary>
        public bool ConnectionStatusBool
        {
            get { return _ConnectionStatusBool; }
            set => Set(ref _ConnectionStatusBool, value);
        }

        /// <summary>
        /// Controller connection status initialisation
        /// </summary>
        public bool ControllerConnectionStatusBool
        {
            get { return _ControllerConnectionStatusBool; }
            set => Set(ref _ControllerConnectionStatusBool, value);
        }

        /// <summary>
        /// Connection status string initialisation
        /// </summary>
        public string ConnectionStatusStr
        {
            get { return _ConnectionStatusStr; }
            set => Set(ref _ConnectionStatusStr, value);
        }

        /// <summary>
        /// Connection butt
        /// </summary>
        public string ConnectToggle
        {
            get { return _ConnectToggle; }
            set => Set(ref _ConnectToggle, value);
        }

        /// <summary>
        /// Waiting for connection to finish or return false
        /// </summary>
        public bool  CanConnect
        {
            get { return _CanConnect; }
            set => Set(ref _CanConnect, value);
        }

        /// <summary>
        /// Rate of translation 
        /// </summary>
        public double TranslationRate
        {
            get { return _TranslationRate; }
            set => Set(ref _TranslationRate, value);
        }

        /// <summary>
        /// Rate of rotation
        /// </summary>
        public double  RotationRate
        {
            get { return _RotationRate; }
            set => Set(ref _RotationRate, value);
        }

        private string _ControllerMoveToggle;

        public string ControllerMoveToggle
        {
            get { return _ControllerMoveToggle; }
            set => Set(ref _ControllerMoveToggle, value);
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

            if (!_socket.Connected)
            {
                Task.Run(() =>
                {
                    var ip = IpAddress;
                    Connect(ip, 30003);
                });
            }
            else if (_socket.Connected)
            {
                Disconnect();
            }
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
                    ConnectionStatusStr = "Connecting";
                    _socket.BeginConnect(new IPEndPoint(ipa, port), ConnectCallback, null   );
                    CanConnect = false;
                }
                else
                {
                    Debug.WriteLine("Already connected to the server!");
                }
            } catch(Exception ex)
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
            RobotJoints = new double[] { 0, 0, 0, 0, 0, 0 };
            RobotPose = new double[] { 0, 0, 0, 0, 0, 0 };

            ConnectionStatusBool = _socket.Connected;
            ConnectionStatusStr = "Stopped";
            ConnectToggle = "Connect";
            CanConnect = true;
        }

        /// <summary>
        /// Connect Callback
        /// </summary>
        /// <param name="result"></param>
        private void ConnectCallback(IAsyncResult result)
        {
            Console.WriteLine($"Status: {_socket.Connected.ToString()}");
            ConnectionStatusStr = "Running";
            CanConnect = true;

            // If can not connect, call disconnect to reinitialize the socket
            if (!_socket.Connected)
                Disconnect();

            try
            {
                if (_socket.Connected)
                {
                    ConnectToggle = "Disconnect";
                    ConnectionStatusBool = _socket.Connected;
                    StartReceiving();
                }
                else
                    Console.WriteLine("Could not connect!");
            }
            catch(Exception ex)
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
                RobotOutputPackage rop = PacketHandler.Handle(packet, packet.Length);

                // Update UI
                UpdateUI(rop);

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
        /// Update UI Values
        /// </summary>
        /// <param name="robotPose"></param>
        public void UpdateUI(RobotOutputPackage robotPackage)
        {
            RobotJoints = robotPackage.RobotJoints;
            RobotPose = robotPackage.RobotPose;
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
        private void SendIO(int io, bool value)
        {
            try
            {
                string data = $"set_digital_out({io},{value})";
                Send(_socket, data);
            }
            catch(SocketException ex)
            {
                Debug.WriteLine($"SendIO exception message: {ex.Message}");
            }
        }

        public void SendScript()
        {
            Send(_socket, Script);
        }

        #endregion

        #region Move Methods

        #region Button Methods

        /// <summary>
        /// Joint Move Buttons
        /// </summary>
        public void J0Add()
        {
            SendMoveCommand("+", 0, "joints");
        }

        public void J0Sub()
        {
            SendMoveCommand("-", 0, "joints");
        }

        public void J1Add()
        {
            SendMoveCommand("+",1, "joints");
        }

        public void J1Sub()
        {
            SendMoveCommand("-", 1, "joints");
        }

        public void J2Add()
        {
            SendMoveCommand("+", 2, "joints");
        }

        public void J2Sub()
        {
            SendMoveCommand("-", 2, "joints");
        }

        public void J3Add()
        {
            SendMoveCommand("+", 3, "joints");
        }

        public void J3Sub()
        {
            SendMoveCommand("-", 3, "joints");
        }

        public void J4Add()
        {
            SendMoveCommand("+", 4, "joints");
        }

        public void J4Sub()
        {
            SendMoveCommand("-", 4, "joints");
        }

        public void J5Add()
        {
            SendMoveCommand("+", 5, "joints");
        }

        public void J5Sub()
        {
            SendMoveCommand("-", 5, "joints");
        }

        /// <summary>
        /// TCP Move Buttons
        /// </summary>
        public void TxAdd()
        {
            SendMoveCommand("+", 0, "tcp");
        }

        public void TxSub()
        {
            SendMoveCommand("-", 0, "tcp");
        }

        public void TyAdd()
        {
            SendMoveCommand("+", 1, "tcp");
        }

        public void TySub()
        {
            SendMoveCommand("-", 1, "tcp");
        }

        public void TzAdd()
        {
            SendMoveCommand("+", 2, "tcp");
        }

        public void TzSub()
        {
            SendMoveCommand("-", 2, "tcp");
        }

        public void RxAdd()
        {
            SendMoveCommand("+", 3, "tcp");
        }

        public void RxSub()
        {
            SendMoveCommand("-", 3, "tcp");
        }

        public void RyAdd()
        {
            SendMoveCommand("+", 4, "tcp");
        }

        public void RySub()
        {
            SendMoveCommand("-", 4, "tcp");
        }

        public void RzAdd()
        {
            SendMoveCommand("+", 5, "tcp");
        }

        public void RzSub()
        {
            SendMoveCommand("-", 5, "tcp");
        }

        #endregion

        /// <summary>
        /// Sending the move command to robot
        /// </summary>
        /// <param name="moveDirection"></param>
        /// <param name="idx"></param>
        private void SendMoveCommand(string moveDirection, int idx, string moveType)
        {
            Task.Run(() =>
            {
                string msg = "";

                // Check the type of move
                if(moveType == "joints")
                {
                    // Check which operation is clicked
                    if (moveDirection == "+")
                        RobotJoints[idx] += TranslationRate;
                    else if (moveDirection == "-")
                        RobotJoints[idx] -= TranslationRate;

                    // Set the string
                    msg = $"movej([" +
                    $"{RobotJoints[0].ToString(new CultureInfo("en-US"))}," +
                    $"{RobotJoints[1].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotJoints[2].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotJoints[3].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotJoints[4].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotJoints[5].ToString(new CultureInfo("en-US"))}]," +
                    $" a = 2, v = 1, t = 0.1)";
                } 
                else if (moveType == "tcp")
                {
                    // Check which operation is clicked and if it is a rotation or translation 
                    if (moveDirection == "+")
                        if(idx < 3 )
                            RobotPose[idx] += TranslationRate;
                        else
                            RobotPose[idx] += RotationRate;
                    else if (moveDirection == "-")
                        if (idx < 3)
                            RobotPose[idx] -= TranslationRate;
                        else
                            RobotPose[idx] -= RotationRate;

                    // Set the string
                    msg = $"movej(p[" +
                    $"{RobotPose[0].ToString(new CultureInfo("en-US"))}," +
                    $"{RobotPose[1].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotPose[2].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotPose[3].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotPose[4].ToString(new CultureInfo("en-US"))}, " +
                    $"{RobotPose[5].ToString(new CultureInfo("en-US"))}]," +
                    $" a = 2, v = 1, t = 0.1)";
                }

                // Send command
                Send(_socket, msg);
            });

        }

        #endregion

        #region Controller

        /// <summary>
        /// Starts the update function and calls it in the set refresh rate
        /// </summary>
        public void StartController()
        {
            _timer.Change(0, 1000 / RefreshRate);
        }

        /// <summary>
        /// Update function
        /// </summary>
        public void ControllerUpdate()
        {
            if (_controller.IsConnected)
            {
                var state = _controller.GetState();

                #region Buttons

                // Increase \ decrease the translation rate
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
                {
                    TranslationRate += 0.002;
                    if (TranslationRate >= 0.1)
                        TranslationRate = 0.1;
                }
                else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                {
                    TranslationRate -= 0.002;
                    if (TranslationRate <= 0.01)
                        TranslationRate = 0.01;
                }

                // Increase \ decrease the rotation rate
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                {
                    RotationRate += 0.002;
                    if (RotationRate >= 0.1)
                        RotationRate = 0.1;
                }
                else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                {
                    RotationRate -= 0.002;
                    if (RotationRate <= 0.01)
                        RotationRate = 0.01;
                }

                // Joints \ TCP Move Toggle
                if(state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start) && !startButtonPressed)
                {
                    startButtonPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);

                    if (ControllerMoveToggle == "TCP")
                        ControllerMoveToggle = "Joints";
                    else
                        ControllerMoveToggle = "TCP";
                }

                startButtonPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);

                #endregion

                #region Translation 

                // Move robot in X axis
                if (state.Gamepad.LeftThumbX >= Gamepad.LeftThumbDeadZone || state.Gamepad.LeftThumbX <= -Gamepad.LeftThumbDeadZone)
                {
                    if(state.Gamepad.LeftThumbX > 0)
                    {
                        if (ControllerMoveToggle == "TCP") TxAdd();
                        if (ControllerMoveToggle == "Joints") J0Add();
                    }
                    else if (state.Gamepad.LeftThumbX < 0)
                    {
                        if (ControllerMoveToggle == "TCP") TxSub();
                        if (ControllerMoveToggle == "Joints") J0Sub();
                    }
                }

                // Move robot in Y axis
                if (state.Gamepad.LeftThumbY >= Gamepad.LeftThumbDeadZone || state.Gamepad.LeftThumbY <= -Gamepad.LeftThumbDeadZone)
                {
                    if (state.Gamepad.LeftThumbY > 0)
                    {
                        if (ControllerMoveToggle == "TCP") TyAdd();
                        if (ControllerMoveToggle == "Joints") J1Add();
                    }
                    else if (state.Gamepad.LeftThumbY < 0)
                    {
                        if (ControllerMoveToggle == "TCP") TySub();
                        if (ControllerMoveToggle == "Joints") J1Sub();
                    }
                }

                // Move robot in Z axis
                if (state.Gamepad.LeftTrigger >= Gamepad.TriggerThreshold)
                {
                    if (state.Gamepad.LeftTrigger > 0)
                    {
                        if (ControllerMoveToggle == "TCP") TzAdd();
                        if (ControllerMoveToggle == "Joints") J2Add();
                    }
                }

                if (state.Gamepad.RightTrigger >= Gamepad.TriggerThreshold)
                {
                    if (state.Gamepad.RightTrigger > 0)
                    {
                        if (ControllerMoveToggle == "TCP") TzSub();
                        if (ControllerMoveToggle == "Joints") J2Sub();
                    }
                }

                #endregion

                #region Rotation

                // Rotate TCP in X axis
                if (state.Gamepad.RightThumbY >= Gamepad.RightThumbDeadZone || state.Gamepad.RightThumbY <= -Gamepad.RightThumbDeadZone)
                {
                    if (state.Gamepad.RightThumbY > 0)
                    {
                        if (ControllerMoveToggle == "TCP") RxAdd();
                        if (ControllerMoveToggle == "Joints") J3Add();
                    }
                    else if (state.Gamepad.RightThumbY < 0)
                    {
                        if (ControllerMoveToggle == "TCP") RxSub();
                        if (ControllerMoveToggle == "Joints") J3Sub();
                    }
                }

                // Rotate TCP in Y axis
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                {
                    if (ControllerMoveToggle == "TCP") RyAdd();
                    if (ControllerMoveToggle == "Joints") J4Add();
                }

                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    if (ControllerMoveToggle == "TCP") RySub();
                    if (ControllerMoveToggle == "Joints") J4Sub();
                }

                // Rotate TCP in Z axis
                if (state.Gamepad.RightThumbX >= Gamepad.RightThumbDeadZone || state.Gamepad.RightThumbX <= -Gamepad.RightThumbDeadZone)
                {
                    if (state.Gamepad.RightThumbX > 0)
                    {
                        if (ControllerMoveToggle == "TCP") RzAdd();
                        if (ControllerMoveToggle == "Joints") J5Add();
                    }
                    else if (state.Gamepad.RightThumbX < 0)
                    {
                        if (ControllerMoveToggle == "TCP") RzSub();
                        if (ControllerMoveToggle == "Joints") J5Sub();
                    }
                }
                #endregion
            }
            //else
            //{
            //    _controller = new Controller(UserIndex.One);
            //}

            ControllerConnectionStatusBool = _controller.IsConnected;
        }
        
        #endregion

    }
}
