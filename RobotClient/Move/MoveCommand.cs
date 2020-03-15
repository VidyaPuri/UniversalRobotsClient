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

namespace RobotClient.Move
{
    public class MoveCommand : PropertyChangedBase, IHandle<RobotOutputPackage>, IHandle<Socket>, IHandle<MoveRateModel>
    {
        private double[] _RobotJoints = { 0, 0, 0, 0, 0, 0 };
        private double[] _RobotPose = { 0, 0, 0, 0, 0, 0 };
        private double _TranslationRate = 0.01;
        private double _RotationRate = 0.01;

        private Socket _socket;

        SocketClient _socketClient;
        IEventAggregator _eventAggregator;

        public MoveCommand(
            IEventAggregator eventAggregator,
            SocketClient socketClient)
        {
            _socketClient = socketClient;
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe(this);
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
        /// Send Move Command
        /// </summary>
        /// <param name="moveDirection"></param>
        /// <param name="idx"></param>
        /// <param name="moveType"></param>
        public void SendMoveCommand(string moveDirection, int idx, string moveType)
        {
            Task.Run(() =>
            {
                string msg = "";

                // Check the type of move
                if (moveType == "joints")
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
                        if (idx < 3)
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
                _socketClient.Send(_socket, msg);
            });
        }

        #region Handlers

        /// <summary>
        /// Robot output package handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(RobotOutputPackage message)
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
            _socket = message;
        }

        /// <summary>
        /// Move rate handler
        /// </summary>
        /// <param name="message"></param>
        public void Handle(MoveRateModel message)
        {
            TranslationRate = message.TranslationRate;
            RotationRate = message.RotationRate;
        }

        #endregion
    }
}
