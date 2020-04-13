using Caliburn.Micro;
using System.Threading.Tasks;
using RobotClient.Models;
using System.Windows;
using RobotClient.Networking;
using RobotClient.Move;

namespace RobotClient.ViewModels
{
    public class ShellViewModel : Screen, IHandle<RobotOutputModel>, IHandle<ConnectionStatusModel>, IHandle<ControllerSettingsModel>, IHandle<MoveRateModel>
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

        private int _Port = 30003;
        private readonly int DashboardPort = 29999;
        private readonly int RoboPort = 4000;

        private string _IpAddress = "192.168.56.102";

        private string _ControllerMoveToggle = "TCP";
        private bool _ControllerConnectionStatusBool;
        private bool _ConnectionStatusBool = false;
        private string _ConnectionStatusStr = "Disconnected";
        private string _ConnectToggle = "Connect";
        private bool _CanConnect = true;
        
        private IEventAggregator _eventAggregator { get; }
        private SocketClient _socketClient;
        private SocketClient _dashboardClient;
        private SocketServer _roboServer = new SocketServer();


        private RobotCommand _robotCommand;
        private ControllerClass _controllerClass;

        private double _TranslationRate = 0.01;
        private double _RotationRate = 0.01;

        private RobotOutputModel _RobotOutputPackage = new RobotOutputModel();
        private MoveRateModel _MoveRate = new MoveRateModel();

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

        public ShellViewModel(
            IEventAggregator eventAggregator,
            SocketClient socketClient,
            RobotCommand moveCommand,
            ControllerClass controllerClass
            )
        {
            _socketClient = socketClient;
            _dashboardClient = new SocketClient(eventAggregator);
            //_roboClient = new SocketClient(eventAggregator);

            _roboServer = new SocketServer();
            _roboServer.StartListening();

            _robotCommand = moveCommand;
            _controllerClass = controllerClass;

            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            
            _controllerClass.StartController();


        }

        #endregion

        #region Properties Initialisation

        /// <summary>
        /// Mover rate model initialisation
        /// </summary>
        public MoveRateModel MoveRate
        {
            get { return _MoveRate; }
            set => Set(ref _MoveRate, value);
        }

        /// <summary>
        /// Robot output package initialisation
        /// </summary>
        public RobotOutputModel RobotOutputPackage
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

        private int myVar;

        public int MyProperty
        {
            get { return myVar; }
            set { myVar = value; }
        }

        /// <summary>
        /// Rate of translation 
        /// </summary>
        public double TranslationRate
        {
            get { return _TranslationRate; }
            set
            {
                _TranslationRate = value;
                if (_TranslationRate > 0.5) _TranslationRate = 0.5;
                if (_TranslationRate < 0.05) _TranslationRate = 0.05;
                NotifyOfPropertyChange(() => TranslationRate);
                MoveRate.TranslationRate = TranslationRate;
                _eventAggregator.PublishOnUIThread(MoveRate);
            }
        }

        /// <summary>
        /// Rate of rotation
        /// </summary>
        public double  RotationRate
        {
            get { return _RotationRate; }
            set
            {
                _RotationRate = value;
                if (_RotationRate > 0.5) _RotationRate = 0.5;
                if (_RotationRate < 0.05) _RotationRate = 0.05;
                NotifyOfPropertyChange(() => RotationRate);
                MoveRate.RotationRate = RotationRate;
                _eventAggregator.PublishOnUIThread(MoveRate);
            }
        }

        /// <summary>
        /// Controller move type toggle
        /// </summary>
        public string ControllerMoveToggle
        {
            get { return _ControllerMoveToggle; }
            set => Set(ref _ControllerMoveToggle, value);
        }

        #endregion

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
                _socketClient.SendIO(0, value);
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
                _socketClient.SendIO(1, value);
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
                _socketClient.SendIO(2, value);
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
                _socketClient.SendIO(3, value);
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
                _socketClient.SendIO(4, value);
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
                _socketClient.SendIO(5, value);
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
                _socketClient.SendIO(6, value);
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
                _socketClient.SendIO(7, value);
            }
        }

        #endregion

        #region Socket Methods

        /// <summary>
        /// ConnectToRobot Button Method
        /// </summary>
        public void ConnectToRobot()
        {
            if (!ConnectionStatusBool)
            {
                Task.Run(() =>
                {
                    var ip = IpAddress;
                    _socketClient.Connect(ip, Port);
                    _dashboardClient.Connect(ip, DashboardPort);
                    //_roboClient.Connect(ip, RoboPort);
                });
            }
            else if (ConnectionStatusBool)
            {
                _socketClient.Disconnect();
                _dashboardClient.Disconnect();
                //_roboClient.Disconnect();
            }
        }

        /// <summary>
        /// Send script button
        /// </summary>
        public void SendScript() { _robotCommand.SendScriptCommand(Script); }   // this one has to be changed

        #endregion

        #region Move Methods

        /// <summary>
        /// Joint Move Buttons
        /// </summary>
        public void J0Add() { _robotCommand.SendSpeedCommand("+", 0, "joints"); }
        public void J0Sub() { _robotCommand.SendSpeedCommand("-", 0, "joints"); }
        public void J1Add() { _robotCommand.SendSpeedCommand("+", 1, "joints"); }
        public void J1Sub() { _robotCommand.SendSpeedCommand("-", 1, "joints"); }
        public void J2Add() { _robotCommand.SendSpeedCommand("+", 2, "joints"); }
        public void J2Sub() { _robotCommand.SendSpeedCommand("-", 2, "joints"); }
        public void J3Add() { _robotCommand.SendSpeedCommand("+", 3, "joints"); }
        public void J3Sub() { _robotCommand.SendSpeedCommand("-", 3, "joints"); }
        public void J4Add() { _robotCommand.SendSpeedCommand("+", 4, "joints"); }
        public void J4Sub() { _robotCommand.SendSpeedCommand("-", 4, "joints"); }
        public void J5Add() { _robotCommand.SendSpeedCommand("+", 5, "joints"); }
        public void J5Sub() { _robotCommand.SendSpeedCommand("-", 5, "joints"); }

        /// <summary>
        /// TCP Move Buttons
        /// </summary>
        public void TxAdd() { _robotCommand.SendSpeedCommand("+", 0, "tcp"); }
        public void TxSub() { _robotCommand.SendSpeedCommand("-", 0, "tcp"); }
        public void TyAdd() { _robotCommand.SendSpeedCommand("+", 1, "tcp"); }
        public void TySub() { _robotCommand.SendSpeedCommand("-", 1, "tcp"); }
        public void TzAdd() { _robotCommand.SendSpeedCommand("+", 2, "tcp"); }
        public void TzSub() { _robotCommand.SendSpeedCommand("-", 2, "tcp"); }
        public void RxAdd() { _robotCommand.SendSpeedCommand("+", 3, "tcp"); }
        public void RxSub() { _robotCommand.SendSpeedCommand("-", 3, "tcp"); }
        public void RyAdd() { _robotCommand.SendSpeedCommand("+", 4, "tcp"); }
        public void RySub() { _robotCommand.SendSpeedCommand("-", 4, "tcp"); }
        public void RzAdd() { _robotCommand.SendSpeedCommand("+", 5, "tcp"); }
        public void RzSub() { _robotCommand.SendSpeedCommand("-", 5, "tcp"); }

        /// <summary>
        /// Enable robot method
        /// </summary>
        public void EnableRobot()
        {
            _robotCommand.EnableRobot();
        }

        /// <summary>
        /// Close popup method
        /// </summary>
        public void ClosePopup()
        {
            _robotCommand.ClosePopup();
        }

        /// <summary>
        /// Send robot to home position
        /// </summary>
        public void Home()
        {
            RobotJoints = new double[] { 0, -1.5708, 1.5708, 0, 1.5708, 0 };
            _robotCommand.SendMoveCommand(RobotJoints, "joints");
        }
        #endregion

        #region Handlers

        /// <summary>
        /// Robot output package handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(RobotOutputModel rop)
        {
            RobotJoints = rop.RobotJoints;
            RobotPose = rop.RobotPose;
        }

        /// <summary>
        /// Connection status handler
        /// </summary>
        /// <param name="status"></param>
        public void Handle(ConnectionStatusModel status)
        {
            CanConnect = status.CanConnect;
            ConnectToggle = status.ConnectToggle;
            ConnectionStatusBool = status.ConnectionStatusBool;
            ConnectionStatusStr = status.ConnectionStatusStr;
        }

        /// <summary>
        /// Controller settings model handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(ControllerSettingsModel message)
        {
            ControllerMoveToggle = message.ControllerMoveToggle;
            ControllerConnectionStatusBool = message.ControllerConnectionStatusBool;
        }

        /// <summary>
        /// Move rate model handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(MoveRateModel message)
        {
            if(RotationRate != message.RotationRate)
                RotationRate = message.RotationRate;
            if(TranslationRate != message.TranslationRate)
                TranslationRate = message.TranslationRate;
        }

        #endregion
    }
}
