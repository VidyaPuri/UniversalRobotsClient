using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using tcpClientWPF.ViewModels;

namespace AsynchronousSockeClient.Networking
{
    public static class PacketHandler
    {
        public static double[] Handle(byte[] packet, int packetSize)
        {
            int off = 8;
            int messageSize = packetSize;
            var q0 = BitConverter.ToDouble(packet, packetSize - 12 - off);
            var q1 = BitConverter.ToDouble(packet, packetSize - 12 - 2 * off);
            var q2 = BitConverter.ToDouble(packet, packetSize - 12 - 3 * off);
            var q3 = BitConverter.ToDouble(packet, packetSize - 12 - 4 * off);
            var q4 = BitConverter.ToDouble(packet, packetSize - 12 - 5 * off);
            var q5 = BitConverter.ToDouble(packet, packetSize - 12 - 6 * off);
            var t0 = BitConverter.ToDouble(packet, packetSize - 12 - 55 * off);
            var t1 = BitConverter.ToDouble(packet, packetSize - 12 - 56 * off);
            var t2 = BitConverter.ToDouble(packet, packetSize - 12 - 57 * off);
            var t3 = BitConverter.ToDouble(packet, packetSize - 12 - 58 * off);
            var t4 = BitConverter.ToDouble(packet, packetSize - 12 - 59 * off);
            var t5 = BitConverter.ToDouble(packet, packetSize - 12 - 60 * off);
            Console.WriteLine($"Received packet! Length: {messageSize}");
            Console.WriteLine($"Received packet! q0: {q0}   t0:{t0}");
            Console.WriteLine($"Received packet! q1: {q1}   t1:{t1}");
            Console.WriteLine($"Received packet! q2: {q2}   t2:{t2}");
            Console.WriteLine($"Received packet! q3: {q3}   t3:{t3}");
            Console.WriteLine($"Received packet! q4: {q4}   t4:{t4}");
            Console.WriteLine($"Received packet! q5: {q5}   t5:{t5}");

            double[] output = { q0, q1, q2, q3, q4, q5 };

            return output;

            //switch(packetType)
            //{
            //    case 2000:
            //        Message msg = new Message(packet);
            //        Console.WriteLine($"Server said: {msg.Text}");
            //        break;
            //}
        }
    }
}

