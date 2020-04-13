using Caliburn.Micro;
using RobotClient.Models;
using SharpDX.XInput;
using System.Threading;

namespace RobotClient.Move
{
    public class ControllerClass : PropertyChangedBase
    {
        private IEventAggregator _eventAggregator { get; }

        private readonly Controller _controller = new Controller(UserIndex.One);
        private readonly int RefreshRate = 60;
        private Timer _timer;

        private ControllerSettingsModel controllerSettingsModel = new ControllerSettingsModel();
        private MoveRateModel moveRateModel = new MoveRateModel();
        private RobotCommand _robotCommand;

        private double _TranslationRate = 0.01;
        private double _RotationRate = 0.01;
        private string ControllerMoveToggle = "TCP";

        private bool buttonStartPressed = false;
        private bool buttonAPressed = false;
        private bool buttonBPressed = false;
        private bool ControllerConnectionStatusBool = false;
        private bool PreviousControllerStatus = false;

        public ControllerClass(
            IEventAggregator eventAggregator,
            RobotCommand robotCommand)
        {
            _eventAggregator = eventAggregator;
            _robotCommand = robotCommand;

            _timer = new Timer(obj => ControllerUpdate());

            ControllerConnectionStatusBool = _controller.IsConnected;
            controllerSettingsModel.ControllerConnectionStatusBool = _controller.IsConnected;
            controllerSettingsModel.ControllerMoveToggle = ControllerMoveToggle;
            moveRateModel.RotationRate = RotationRate;
            moveRateModel.TranslationRate = TranslationRate;
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
        public double RotationRate
        {
            get { return _RotationRate; }
            set => Set(ref _RotationRate, value);
        }

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
                    TranslationRate += 0.005;
                    if (TranslationRate >= 0.5)
                        TranslationRate = 0.5;

                    moveRateModel.TranslationRate = TranslationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }
                else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                {
                    TranslationRate -= 0.005;
                    if (TranslationRate <= 0.05)
                        TranslationRate = 0.05;

                    moveRateModel.TranslationRate = TranslationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }

                // Increase \ decrease the rotation rate
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                {
                    RotationRate += 0.005;
                    if (RotationRate >= 0.5)
                        RotationRate = 0.5;

                    moveRateModel.RotationRate = RotationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }
                else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                {
                    RotationRate -= 0.005;
                    if (RotationRate <= 0.05)
                        RotationRate = 0.05;

                    moveRateModel.RotationRate = RotationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }

                // Joints \ TCP Move Toggle
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start) && !buttonStartPressed)
                {
                    buttonStartPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);

                    if (ControllerMoveToggle == "TCP")
                        ControllerMoveToggle = "Joints";
                    else
                        ControllerMoveToggle = "TCP";

                    controllerSettingsModel.ControllerMoveToggle = ControllerMoveToggle;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }

                // Close popup when Button A is pressed
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && !buttonAPressed)
                {
                    buttonAPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                    _robotCommand.ClosePopup();
                }

                // Enable robot when Button B is pressed
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B) && !buttonBPressed)
                {
                    buttonAPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                    _robotCommand.EnableRobot();
                }


                // reinitialize button states
                buttonStartPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);
                buttonAPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                buttonAPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);

                #endregion

                #region Translation 

                // Move robot in X axis
                if (state.Gamepad.LeftThumbX >= Gamepad.LeftThumbDeadZone || state.Gamepad.LeftThumbX <= -Gamepad.LeftThumbDeadZone)
                {
                    if (state.Gamepad.LeftThumbX > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("+", 0, "tcp"); ;
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("+", 0, "joints");
                    }
                    else if (state.Gamepad.LeftThumbX < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("-", 0, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("-", 0, "joints");
                    }
                }

                // Move robot in Y axis
                if (state.Gamepad.LeftThumbY >= Gamepad.LeftThumbDeadZone || state.Gamepad.LeftThumbY <= -Gamepad.LeftThumbDeadZone)
                {
                    if (state.Gamepad.LeftThumbY > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("+", 1, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("+", 1, "joints");
                    }
                    else if (state.Gamepad.LeftThumbY < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("-", 1, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("-", 1, "joints");
                    }
                }

                // Move robot in Z axis
                if (state.Gamepad.RightTrigger >= Gamepad.TriggerThreshold)
                {
                    if (state.Gamepad.RightTrigger > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("+", 2, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("+", 2, "joints");
                    }
                }

                if (state.Gamepad.LeftTrigger >= Gamepad.TriggerThreshold)
                {
                    if (state.Gamepad.LeftTrigger > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("-", 2, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("-", 2, "joints");
                    }
                }
                #endregion

                #region Rotation

                // Rotate TCP in X axis
                if (state.Gamepad.RightThumbY >= Gamepad.RightThumbDeadZone || state.Gamepad.RightThumbY <= -Gamepad.RightThumbDeadZone)
                {
                    if (state.Gamepad.RightThumbY > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("+", 3, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("+", 3, "joints");
                    }
                    else if (state.Gamepad.RightThumbY < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("-", 3, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("-", 3, "joints");
                    }
                }

                // Rotate TCP in Y axis
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                {
                    if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("+", 4, "tcp");
                    if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("+", 4, "joints");
                }

                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("-", 4, "tcp");
                    if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("-", 4, "joints");
                }

                // Rotate TCP in Z axis
                if (state.Gamepad.RightThumbX >= Gamepad.RightThumbDeadZone || state.Gamepad.RightThumbX <= -Gamepad.RightThumbDeadZone)
                {
                    if (state.Gamepad.RightThumbX > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("+", 5, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("+", 5, "joints");
                    }
                    else if (state.Gamepad.RightThumbX < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _robotCommand.SendSpeedCommand("-", 5, "tcp");
                        if (ControllerMoveToggle == "Joints") _robotCommand.SendSpeedCommand("-", 5, "joints");
                    }
                }
                #endregion
            }

            ControllerConnectionStatusBool = _controller.IsConnected;

            // On change send the new status
            if (ControllerConnectionStatusBool != PreviousControllerStatus)
            {
                ControllerConnectionStatusBool = _controller.IsConnected;
                controllerSettingsModel.ControllerConnectionStatusBool = ControllerConnectionStatusBool;
                PreviousControllerStatus = ControllerConnectionStatusBool;
                PublishEventToUI();
            }
        }

        /// <summary>
        /// Publish on UI
        /// </summary>
        private void PublishEventToUI()
        {
            _eventAggregator.BeginPublishOnUIThread(moveRateModel);
            _eventAggregator.BeginPublishOnUIThread(controllerSettingsModel);
        }
    }

}
