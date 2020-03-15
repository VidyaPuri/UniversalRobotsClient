using Caliburn.Micro;
using RobotClient.Models;
using RobotClient.Move;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RobotClient.Move
{
    public class ControllerClass : PropertyChangedBase
    {
        private IEventAggregator _eventAggregator { get; }

        private readonly Controller _controller = new Controller(UserIndex.One);
        private readonly int RefreshRate = 60;
        private Timer _timer;

        private ControllerSettingsModel moveRateModel = new ControllerSettingsModel();
        private MoveCommand _moveCommand;

        private double _TranslationRate = 0.1;
        private double _RotationRate = 0.1;
        private string ControllerMoveToggle = "TCP";

        private bool startButtonPressed = false;
        private bool ControllerConnectionStatusBool = false;

        public ControllerClass(
            IEventAggregator eventAggregator,
            MoveCommand moveCommand)
        {
            _eventAggregator = eventAggregator;
            _moveCommand = moveCommand;

            _timer = new Timer(obj => ControllerUpdate());

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
                    TranslationRate += 0.002;
                    if (TranslationRate >= 0.1)
                        TranslationRate = 0.1;

                    moveRateModel.TranslationRate = TranslationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }
                else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                {
                    TranslationRate -= 0.002;
                    if (TranslationRate <= 0.01)
                        TranslationRate = 0.01;

                    moveRateModel.TranslationRate = TranslationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }

                // Increase \ decrease the rotation rate
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                {
                    RotationRate += 0.002;
                    if (RotationRate >= 0.1)
                        RotationRate = 0.1;

                    moveRateModel.RotationRate = RotationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }
                else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                {
                    RotationRate -= 0.002;
                    if (RotationRate <= 0.01)
                        RotationRate = 0.01;

                    moveRateModel.RotationRate = RotationRate;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }

                // Joints \ TCP Move Toggle
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start) && !startButtonPressed)
                {
                    startButtonPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);

                    if (ControllerMoveToggle == "TCP")
                        ControllerMoveToggle = "Joints";
                    else
                        ControllerMoveToggle = "TCP";

                    moveRateModel.ControllerMoveToggle = ControllerMoveToggle;
                    _eventAggregator.BeginPublishOnUIThread(moveRateModel);
                }

                startButtonPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);

                #endregion

                #region Translation 

                // Move robot in X axis
                if (state.Gamepad.LeftThumbX >= Gamepad.LeftThumbDeadZone || state.Gamepad.LeftThumbX <= -Gamepad.LeftThumbDeadZone)
                {
                    if (state.Gamepad.LeftThumbX > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 0, "joints"); ;
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 0, "joints");
                    }
                    else if (state.Gamepad.LeftThumbX < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 1, "joints");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 1, "joints");
                    }
                }

                // Move robot in Y axis
                if (state.Gamepad.LeftThumbY >= Gamepad.LeftThumbDeadZone || state.Gamepad.LeftThumbY <= -Gamepad.LeftThumbDeadZone)
                {
                    if (state.Gamepad.LeftThumbY > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 2, "joints");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 2, "joints");
                    }
                    else if (state.Gamepad.LeftThumbY < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 3, "joints");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 3, "joints");
                    }
                }

                // Move robot in Z axis
                if (state.Gamepad.LeftTrigger >= Gamepad.TriggerThreshold)
                {
                    if (state.Gamepad.LeftTrigger > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 4, "joints");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 4, "joints");
                    }
                }

                if (state.Gamepad.RightTrigger >= Gamepad.TriggerThreshold)
                {
                    if (state.Gamepad.RightTrigger > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 5, "joints");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 5, "joints");
                    }
                }

                #endregion

                #region Rotation

                // Rotate TCP in X axis
                if (state.Gamepad.RightThumbY >= Gamepad.RightThumbDeadZone || state.Gamepad.RightThumbY <= -Gamepad.RightThumbDeadZone)
                {
                    if (state.Gamepad.RightThumbY > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 0, "tcp");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 0, "tcp");
                    }
                    else if (state.Gamepad.RightThumbY < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 1, "tcp");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 1, "tcp");
                    }
                }

                // Rotate TCP in Y axis
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                {
                    if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 2, "tcp");
                    if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 2, "tcp");
                }

                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 3, "tcp");
                    if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 3, "tcp");
                }

                // Rotate TCP in Z axis
                if (state.Gamepad.RightThumbX >= Gamepad.RightThumbDeadZone || state.Gamepad.RightThumbX <= -Gamepad.RightThumbDeadZone)
                {
                    if (state.Gamepad.RightThumbX > 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 4, "tcp");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 4, "tcp");
                    }
                    else if (state.Gamepad.RightThumbX < 0)
                    {
                        if (ControllerMoveToggle == "TCP") _moveCommand.SendMoveCommand("+", 5, "tcp");
                        if (ControllerMoveToggle == "Joints") _moveCommand.SendMoveCommand("-", 5, "tcp");
                    }
                }
                #endregion

                // First time initialisation
                if(!ControllerConnectionStatusBool)
                {
                    moveRateModel.ControllerConnectionStatusBool = _controller.IsConnected;
                    moveRateModel.ControllerMoveToggle = ControllerMoveToggle;
                    moveRateModel.RotationRate = RotationRate;
                    moveRateModel.TranslationRate = TranslationRate;
                    PublishEventToUI();
                }
            }
            else
            {
                ControllerConnectionStatusBool = _controller.IsConnected;
                PublishEventToUI();
            }
        }



        private void PublishEventToUI()
        {
            _eventAggregator.BeginPublishOnUIThread(moveRateModel);
        }
    }

}
