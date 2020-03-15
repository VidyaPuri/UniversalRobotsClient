using System;
using RobotClient.Models;

namespace AsynchronousSockeClient.Networking
{
    public static class PacketHandler
    {
        public static RobotOutputModel Handle(byte[] packet, int packetSize)
        {
            RobotOutputModel output = new RobotOutputModel();

            // Lenght offset
            int integerOffset = 4;
            int doubleOffset = 8;

            // Msg size 4bytes
            var msgSize = BitConverter.ToUInt32(packet, packetSize - integerOffset);

            // Time since start of robot
            var time = BitConverter.ToDouble(packet, packetSize - integerOffset - 1 * doubleOffset);

            // Joint values
            var q0 = BitConverter.ToDouble(packet, packetSize - integerOffset - 2 * doubleOffset);
            var q1 = BitConverter.ToDouble(packet, packetSize - integerOffset - 3 * doubleOffset);
            var q2 = BitConverter.ToDouble(packet, packetSize - integerOffset - 4 * doubleOffset);
            var q3 = BitConverter.ToDouble(packet, packetSize - integerOffset - 5 * doubleOffset);
            var q4 = BitConverter.ToDouble(packet, packetSize - integerOffset - 6 * doubleOffset);
            var q5 = BitConverter.ToDouble(packet, packetSize - integerOffset - 7 * doubleOffset);

            // TCP values
            var t0 = BitConverter.ToDouble(packet, packetSize - integerOffset - 56 * doubleOffset);
            var t1 = BitConverter.ToDouble(packet, packetSize - integerOffset - 57 * doubleOffset);
            var t2 = BitConverter.ToDouble(packet, packetSize - integerOffset - 58 * doubleOffset);
            var t3 = BitConverter.ToDouble(packet, packetSize - integerOffset - 59 * doubleOffset);
            var t4 = BitConverter.ToDouble(packet, packetSize - integerOffset - 60 * doubleOffset);
            var t5 = BitConverter.ToDouble(packet, packetSize - integerOffset - 61 * doubleOffset);


            output.RobotPose = new double[] { t0, t1, t2, t3, t4, t5 };
            output.RobotJoints = new double[] { q0, q1, q2, q3, q4, q5 };

            return output;
        }
    }
}

