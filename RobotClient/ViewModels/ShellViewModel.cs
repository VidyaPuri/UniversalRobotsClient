using Caliburn.Micro;
using System.Threading.Tasks;
using RobotClient.Models;
using System.Windows;
using RobotClient.Networking;
using RobotClient.Move;
using RobotInterface.Models;
using System;
using System.IO.Ports;
using System.Diagnostics;
using RobotInterface.Networking;
using System.Text;
using RobotInterface.Helpers;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace RobotClient.ViewModels
{
    public class ShellViewModel : Screen, IHandle<RobotOutputModel>, IHandle<ConnectionStatusModel>, IHandle<ControllerSettingsModel>, IHandle<MoveRateModel>, IHandle<int>, IHandle<SerialStatusModel>, IHandle<LogModel>, IHandle<Clock>
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

        // Socket initialisers
        private int _Port = 30003;
        private readonly int DashboardPort = 29999;
        private readonly int RoboPort = 11000;
        private string _IpAddress = "192.168.56.102";

        private string _ControllerMoveToggle = "TCP";
        private bool _ControllerConnectionStatusBool;
        private bool _ConnectionStatusBool = false;
        private string _ConnectionStatusStr = "Disconnected";
        private string _ConnectToggle = "Connect";
        private bool _CanConnect = true;

        // EventAggregator
        private IEventAggregator _eventAggregator { get; }

        // Network
        private SocketClient _socketClient;
        private SocketClient _dashboardClient;
        private SocketServer _roboServer;
        private SerialCommunication _serial;
        private BluetoothConnection _BTConnection;

        private BindableCollection<FocusModel> _FocusList = new BindableCollection<FocusModel>();

        private int _ReceivedFocusTarget;
        private int _SelectedFocusTargetIdx = 0;
        private double _SliderValue = 1500;

        private RobotCommand _robotCommand;
        private ControllerClass _controllerClass;

        // Move rates
        private double _TranslationRate = 0.01;
        private double _RotationRate = 0.01;

        // Models
        private RobotOutputModel _RobotOutputPackage = new RobotOutputModel();
        private MoveRateModel _MoveRate = new MoveRateModel();

        // Initial values
        private double[] _RobotJoints = { 0, 0, 0, 0, 0, 0 };
        private double[] _RobotPose = { 0, 0, 0, 0, 0, 0 };

        // IO initialisers
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

            _roboServer = new SocketServer(eventAggregator);

            _robotCommand = moveCommand;
            _controllerClass = controllerClass;

            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            DialogEventAggregatorProvider.EG.Subscribe(this);

            _controllerClass.StartController();

            _serial = new SerialCommunication(eventAggregator);
            _BTConnection = new BluetoothConnection(eventAggregator);

            ComPortList = SerialPort.GetPortNames();
            BaudRateList = DataLists.GetBaudRates();
            MotorStepTypeList = DataLists.GetStepTypes();

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
        public bool CanConnect
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
        public double RotationRate
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

        /// <summary>
        /// List with Focus Targets
        /// </summary>
        public BindableCollection<FocusModel> FocusList
        {
            get { return _FocusList; }
            set => Set(ref _FocusList, value);
        }

        /// <summary>
        /// Idx of selected focus target
        /// </summary>
        public int SelectedFocusTargetIdx
        {
            get { return _SelectedFocusTargetIdx; }
            set => Set(ref _SelectedFocusTargetIdx, value);
        }

        /// <summary>
        /// USB Slider Value init
        /// </summary>
        public double SliderValue
        {
            get { return _SliderValue; }
            set
            {
                _SliderValue = value;
                _serial.SendToPort(value);
                NotifyOfPropertyChange(() => SliderValue);
            }
        }

        /// <summary>
        /// The target that was received from UR
        /// </summary>
        public int ReceivedFocusTarget
        {
            get { return _ReceivedFocusTarget; }
            set
            {
                _ReceivedFocusTarget = value;
                Debug.WriteLine($"I have received the order to execute servo focus position #{ReceivedFocusTarget}");
                try
                {
                    _serial.SendToPort(FocusList[value].Val);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                NotifyOfPropertyChange(() => ReceivedFocusTarget);
            }
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

        public object Slider_Value { get; private set; }

        #endregion

        #region Socket Methods

        public void StartSocketServer()
        {
            Task.Run(() =>
            {
                _roboServer.StartListening(RoboPort);
            });
        }

        public void StopSocketServer()
        {
            _roboServer.CloseServer();
        }

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

        #region Target Focus Methods

        /// <summary>
        /// Adds new focus target to the list
        /// </summary>
        public void AddFocusTarget()
        {
            var target = new FocusModel
            {
                Name = "GetSomeName",
                Idx = 0,
                Val = SliderValue
            };
            FocusList.Add(target);
        }

        /// <summary>
        /// Insert new focus target to the list
        /// </summary>
        public void InsertFocusTarget()
        {
            var target = new FocusModel
            {
                Name = "FocusTarget",
                Idx = 0,
                Val = SliderValue
            };
            FocusList.Insert(SelectedFocusTargetIdx, target);
        }

        /// <summary>
        /// Edits selected focus target
        /// </summary>
        public void EditFocusTarget()
        {
            if (FocusList.Count > 0)
                FocusList[SelectedFocusTargetIdx].Val = SliderValue;

            FocusList.Refresh();
        }

        /// <summary>
        /// Removes selected focus target from the list
        /// </summary>
        public void RemoveFocusTarget()
        {
            if (FocusList.Count > 0)
                FocusList.RemoveAt(SelectedFocusTargetIdx);
            SelectedFocusTargetIdx = 0;
        }

        #endregion

        #region USB Serial Communication Methods

        #region Private Members

        private bool _USBSerialStatus;
        private string _USBConnectBtnText = "Open Port";

        #endregion

        #region USB Property Initialisation 

        /// <summary>
        /// USB Serial status
        /// </summary>
        public bool USBSerialStatus
        {
            get { return _USBSerialStatus; }
            set => Set(ref _USBSerialStatus, value);
        }

        /// <summary>
        /// USB Connect serial btn text
        /// </summary>
        public string USBConnectBtnText
        {
            get { return _USBConnectBtnText; }
            set => Set(ref _USBConnectBtnText, value);
        }

        #endregion

        /// <summary>
        /// Open up the serial port
        /// </summary>
        public void USBSerialConnect()
        {
            try
            {
                // Async Task for connecting
                Task.Run(() =>
                {
                    if (!USBSerialStatus)
                    {
                        _serial.OpenSerialPort();
                        //BTSerialStatus = true;
                    }
                    else if (USBSerialStatus)
                    {
                        _serial.CloseSerialPort();
                        //BTSerialStatus = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion

        #region Test stuff

        private string _SerialInput;

        public string SerialInput
        {
            get { return _SerialInput; }
            set => Set(ref _SerialInput, value);
        }


        public void SendSerial()
        {
            var value = Convert.ToDouble(SerialInput);
            _serial.SendToPort(value);

        }
        #endregion

        #region Bluetooth

        #region Bluetooth Private Members

        // Private BT properties 
        private string _BluetoothInputText;
        private BindableCollection<LogModel> _BTReceivedMessage = new BindableCollection<LogModel>();
        private bool _BTSerialStatus = false;
        private string _BTConnectBtnText = "Connect";

        private string[] _ComPortList;
        private string _SelectedComPort;
        private int _SelectedComPortIndex = 0;
        private string[] _BaudRateList;
        private string _SelectedBaudRate;
        private int _BaudRateIndex = 6;

        private double _BTSliderVal = 0;

        #endregion

        #region Bluetooth Properties Initialisation

        /// <summary>
        /// BT Input text initialisation
        /// </summary>
        public string BluetoothInputText
        {
            get { return _BluetoothInputText; }
            set => Set(ref _BluetoothInputText, value);
        }

        /// <summary>
        /// LogModel List
        /// </summary>
        public BindableCollection<LogModel> BTReceivedMessage
        {
            get { return _BTReceivedMessage; }
            set => Set(ref _BTReceivedMessage, value);
        }

        /// <summary>
        /// Serial Status Initialisation
        /// </summary>
        public bool BTSerialStatus
        {
            get { return _BTSerialStatus; }
            set => Set(ref _BTSerialStatus, value);
        }

        /// <summary>
        /// ConnectBtn Text 
        /// </summary>
        public string BTConnectBtnText
        {
            get { return _BTConnectBtnText; }
            set => Set(ref _BTConnectBtnText, value);
        }

        /// <summary>
        /// ComPortList
        /// </summary>
        public string[] ComPortList
        {
            get { return _ComPortList; }
            set => Set(ref _ComPortList, value);
        }

        /// <summary>
        /// SelectedComPort
        /// </summary>
        public string SelectedComPort
        {
            get { return _SelectedComPort; }
            set => Set(ref _SelectedComPort, value);
        }


        /// <summary>
        /// SelectedComPortIndex
        /// </summary>
        public int SelectedComPortIndex
        {
            get { return _SelectedComPortIndex; }
            set => Set(ref _SelectedComPortIndex, value);
        }

        /// <summary>
        /// BaudRateList
        /// </summary>
        public string[] BaudRateList
        {
            get { return _BaudRateList; }
            set => Set(ref _BaudRateList, value);
        }

        /// <summary>
        /// SelectedBaudRate
        /// </summary>
        public string SelectedBaudRate
        {
            get { return _SelectedBaudRate; }
            set => Set(ref _SelectedBaudRate, value);
        }

        /// <summary>
        /// SelectedBaudRateIndex
        /// </summary>
        public int BaudRateIndex
        {
            get { return _BaudRateIndex; }
            set => Set(ref _BaudRateIndex, value);
        }

        /// <summary>
        /// Bluetooth slider value
        /// </summary>
        public double BTSliderVal
        {
            get { return _BTSliderVal; }
            set
            {
                _BTSliderVal = value;
                NotifyOfPropertyChange(() => BTSliderVal);
                _BTConnection.SendStepperString(MotorStepType, StepMotorSpeed, Convert.ToInt32(value));
            }
        }

        #endregion

        #region Bluetooth Methods

        /// <summary>
        /// Connect BT
        /// </summary>
        public void BTSerialConnect()
        {
            try
            {
                // Async Task for connecting
                Task.Run(() =>
                {
                    if (!BTSerialStatus)
                    {
                        _BTConnection.Connect(SelectedComPort, SelectedBaudRate);
                        //BTSerialStatus = true;
                    }
                    else if (BTSerialStatus)
                    {
                        _BTConnection.Disconnect();
                        //BTSerialStatus = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Send string to arduino via BT
        /// </summary>
        public void SendToArduinoBlueTooth()
        {
            if (BTSerialStatus)
            {
                try
                {
                    _BTConnection.SendString(BluetoothInputText);
                    BluetoothInputText = string.Empty;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        #endregion

        #endregion

        #region Stepper Motor Module

        private string[] _MotorStepTypeList;
        private string _MotorStepType;
        private int _MotorStepTypeIdx;
        private int _StepMotorSpeed = 0;

        /// <summary>
        /// Motor step type
        /// </summary>
        public string MotorStepType
        {
            get { return _MotorStepType; }
            set => Set(ref _MotorStepType, value);
        }

        /// <summary>
        /// Motor step type index
        /// </summary>
        public int MotorStepTypeIdx
        {
            get { return _MotorStepTypeIdx; }
            set => Set(ref _MotorStepTypeIdx, value);
        }

        /// <summary>
        /// Motor step type list
        /// </summary>
        public string[] MotorStepTypeList
        {
            get { return _MotorStepTypeList; }
            set => Set(ref _MotorStepTypeList, value);
        }

        /// <summary>
        /// Stepper motor speed in RPM
        /// </summary>
        public int StepMotorSpeed
        {
            get { return _StepMotorSpeed; }
            set => Set(ref _StepMotorSpeed, value);
        }


        #endregion

        #region Timeline

        #region Timeline Properties

        private int _FloaterPos = 0;
        private bool _MouseStatus = false;
        private int _MousePosX = 0;
        private Clock _Clock;


        public int FloaterPos
        {
            get { return _FloaterPos; }
            set => Set(ref _FloaterPos, value);
        }

        public bool MouseStatus
        {
            get { return _MouseStatus; }
            set => Set(ref _MouseStatus, value);
        }


        public int MousePosX
        {
            get { return _MousePosX; }
            set => Set(ref _MousePosX, value);
        }


        public void MouseMove(Canvas source)
        {
            if(MouseStatus)
            {
                Point p = Mouse.GetPosition(source);
                MousePosX = Convert.ToInt32(p.X);
                FloaterPos = MousePosX;
            }
        }

        public void MouseDown()
        {
            MouseStatus = true;
        }

        public void MouseUp()
        {
            MouseStatus = false;

        }


        public Clock Clock
        {
            get { return _Clock; }
            set { _Clock = value; }
        }
        public void MyEventHandler(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("this was done");
        }

        private string _CurrentTime;

        public string CurrentTime
        {
            get { return _CurrentTime; }
            set 
            { 
                _CurrentTime = value;
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }



        #endregion

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

        /// <summary>
        /// Received focus target handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(int message)
        {
            ReceivedFocusTarget = message;
        }

        /// <summary>
        /// Reeived message from BT string
        /// </summary>
        /// <param name="message"></param>
        public void Handle(LogModel message)
        {
            BTReceivedMessage.Add(message);
        }

        public void Handle(SerialStatusModel message)
        {
            if(message.ComType == "BT")
                BTSerialStatus = message.BTSerialStatus;
            if(message.ComType == "USB")
                USBSerialStatus = message.USBSerialStatus;

            // BT Serial Status
            if (BTSerialStatus)
                BTConnectBtnText = "Disconnect";
            else if (!BTSerialStatus)
                BTConnectBtnText = "Connect";

            // USB Serial Status
            if (USBSerialStatus)
                USBConnectBtnText = "Close Port";
            else if (!USBSerialStatus)
                USBConnectBtnText = "Open Port";
        }

        /// <summary>
        /// CurrentTimeInvalidatedEventHandler's message
        /// </summary>
        /// <param name="message"></param>
        public void Handle(Clock message)
        {
            Debug.WriteLine($"Current time is: {message.CurrentTime}");
            CurrentTime = message.CurrentTime.ToString();
        }

        #endregion
    }
}
