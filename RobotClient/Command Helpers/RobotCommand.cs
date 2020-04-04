using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Threading.Tasks;
using System.Globalization;
using RobotClient.Networking;
using RobotClient.Models;
using System.Net.Sockets;
using System.Diagnostics;

namespace RobotClient.Move
{
    public class RobotCommand : PropertyChangedBase, IHandle<RobotOutputModel>, IHandle<Socket>, IHandle<MoveRateModel>
    {
        #region Private members
        private double[] JointSpeed { get; set; } = { 0, 0, 0, 0, 0, 0 };
        private double[] ToolSpeed { get; set; } = { 0, 0, 0, 0, 0, 0 };
        
        private double[] _RobotJoints = { 0, 0, 0, 0, 0, 0 };
        private double[] _RobotPose = { 0, 0, 0, 0, 0, 0 };
        private double _TranslationRate = 0.01;
        private double _RotationRate = 0.01;

        private string command;

        private Socket _rtSocket;
        private Socket _dashSocket;
        SocketClient _socketClient;
        IEventAggregator _eventAggregator;

        #endregion

        #region Constructor

        public RobotCommand(
            IEventAggregator eventAggregator,
            SocketClient socketClient)
        {
            _socketClient = socketClient;
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe(this);
        }

        #endregion

        #region Public property initialisation

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

        #endregion

        #region Move Commands

        /// <summary>
        /// Send Move Command
        /// </summary>
        /// <param name="moveDirection"></param>
        /// <param name="idx"></param>
        /// <param name="moveType"></param>
        public void SendMoveCommand(double[] position, string moveType)
        {
            Task.Run(() =>
            {
                string msg = "";

                // Check the type of move
                if (moveType == "joints")
                {
                    // Set the string
                    msg = $"movej([" +
                    $"{position[0].ToString(new CultureInfo("en-US"))}," +
                    $"{position[1].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[2].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[3].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[4].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[5].ToString(new CultureInfo("en-US"))}]," +
                    $" t = 2)";
                }
                else if (moveType == "tcp")
                {
                    // Set the string
                    msg = $"movej(p[" +
                    $"{position[0].ToString(new CultureInfo("en-US"))}," +
                    $"{position[1].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[2].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[3].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[4].ToString(new CultureInfo("en-US"))}, " +
                    $"{position[5].ToString(new CultureInfo("en-US"))}]," +
                    $" t = 2)";
                }

                // Send command
                _socketClient.Send(_rtSocket, msg);
            });
        }

        /// <summary>
        /// Send speed command
        /// </summary>
        /// <param name="moveDirection"></param>
        /// <param name="idx"></param>
        /// <param name="moveType"></param>
        public void SendSpeedCommand(string moveDirection, int idx, string moveType)
        {
            Task.Run(() =>
            {
                string msg = "";

                // Check the type of move
                if (moveType == "joints")
                {
                    // Check which operation is clicked
                    if (moveDirection == "+")
                        JointSpeed[idx] = TranslationRate;
                    else if (moveDirection == "-")
                        JointSpeed[idx] = -TranslationRate;

                    // Set the string
                     msg = $"speedj([" +
                    $"{JointSpeed[0].ToString(new CultureInfo("en-US"))}," +
                    $"{JointSpeed[1].ToString(new CultureInfo("en-US"))}, " +
                    $"{JointSpeed[2].ToString(new CultureInfo("en-US"))}, " +
                    $"{JointSpeed[3].ToString(new CultureInfo("en-US"))}, " +
                    $"{JointSpeed[4].ToString(new CultureInfo("en-US"))}, " +
                    $"{JointSpeed[5].ToString(new CultureInfo("en-US"))}]," +
                    $" 0.5,0.1)";
                }
                else if (moveType == "tcp")
                {
                    // Check which operation is clicked and if it is a rotation or translation 
                    if (moveDirection == "+")
                        if (idx < 3)
                            ToolSpeed[idx] = TranslationRate;
                        else
                            ToolSpeed[idx] = RotationRate;
                    else if (moveDirection == "-")
                        if (idx < 3)
                            ToolSpeed[idx] = -TranslationRate;
                        else
                            ToolSpeed[idx] = -RotationRate;

                    // Set the string
                    msg = $"speedl([" +
                    $"{ToolSpeed[0].ToString(new CultureInfo("en-US"))}," +
                    $"{ToolSpeed[1].ToString(new CultureInfo("en-US"))}, " +
                    $"{ToolSpeed[2].ToString(new CultureInfo("en-US"))}, " +
                    $"{ToolSpeed[3].ToString(new CultureInfo("en-US"))}, " +
                    $"{ToolSpeed[4].ToString(new CultureInfo("en-US"))}, " +
                    $"{ToolSpeed[5].ToString(new CultureInfo("en-US"))}]," +
                    $" 0.5,0.1)";
                }

                // Send command
                _socketClient.Send(_rtSocket, msg);
                Debug.WriteLine($"Output message: Movetype: {moveType} and the whole script string: {msg}");
                JointSpeed = new double[] { 0, 0, 0, 0, 0, 0 };
                ToolSpeed = new double[] { 0, 0, 0, 0, 0, 0 };
            });
        }

        /// <summary>
        /// Sends the script command
        /// </summary>
        /// <param name="script"></param>
        public void SendScriptCommand(string script)
        {
            _socketClient.Send(_dashSocket, script);
        }

        #endregion

        #region Dashboard Commands

        /// <summary>
        /// Releasing the protective stop break
        /// </summary>
        public void EnableRobot()
        {
            command = "unlock protective stop";
            _socketClient.Send(_dashSocket, command);
        }

        /// <summary>
        /// Closing the popups
        /// </summary>
        public void ClosePopup()
        {
            command = "close safety popup";
            _socketClient.Send(_dashSocket, command);
            command = "close popup";
            _socketClient.Send(_dashSocket, command);
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Robot output package handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(RobotOutputModel message)
        {
            RobotPose = message.RobotPose;
            RobotJoints = message.RobotJoints;
        }

        /// <summary>
        /// Socket handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(Socket message)
        {
            try
            {
                if (message.RemoteEndPoint.ToString() == "192.168.56.101:30003")
                    _rtSocket = message;
                else if (message.RemoteEndPoint.ToString() == "192.168.56.101:29999")
                    _dashSocket = message;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary> "192.168.56.101:29999"
        /// Move rate handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(MoveRateModel message)
        {
            if (RotationRate != message.RotationRate)
                RotationRate = message.RotationRate;
            if (TranslationRate != message.TranslationRate)
                TranslationRate = message.TranslationRate;
        }

        #endregion
    }
}
