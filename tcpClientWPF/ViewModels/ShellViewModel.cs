using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace tcpClientWPF.ViewModels
{
    public class ShellViewModel : Screen
    {
        public ShellViewModel()
        {
            //StartClient();

        }

        private string _IpAddress = "192.168.58.101";

        public string IpAddress
        {
            get { return _IpAddress; }
            set => Set(ref _IpAddress, value); 
        }

        private int _Port = 30003;

        public int Port
        {
            get => _Port;
            set => Set(ref _Port, value);
        }

        private double[] _RobotPose = { 0, 0, 0, 0, 0, 0 };   

        public double[] RobotPose
        {
            get => _RobotPose; 
            set => Set(ref _RobotPose, value);
        }


        public void ConnectToRobot()
        {
           Debug.WriteLine($"IpAddress: { IpAddress}");
           Debug.WriteLine($"Port: { Port}");
            StartClient();

        }

        //public void CloseClient()
        //{
        //    // Release the socket.
        //    sender.Shutdown(SocketShutdown.Both);
        //    sender.Close();
        //}

        public void StartClient()
        {
            byte[] bytes = new byte[1116];
            try
            {
                // Connect to a Remote server
                // Get Host IP Address that is used to establish a connection
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1
                // If a host has multiple addresses, you will get a list of addresses
                //IPHostEntry host = Dns.GetHostEntry("localhost");
                //IPAddress ipAddress = host.AddressList[0];
                //IPAddress iPAddress = IPAddress.Parse(IpAddress);
                //IPEndPoint remoteEP = new IPEndPoint(iPAddress, Port);
                string ip = IpAddress;
                IPAddress iPAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(iPAddress, Port);
                // Create a TCP/IP  socket.
                Socket sender = new Socket(iPAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    // Connect to Remote EndPoint
                    //sender.Connect(remoteEP);
                    sender.Connect(remoteEP);
                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());
                    // Encode the data string into a byte array.
                    //byte[] msg = Encoding.ASCII.GetBytes("movej([-1.95, 1.58, 1.16, -1.15, -1.55, 1.18], a=1.0, v=0.1)");
                    //Task.Delay(100);
                    byte[] msg2 = Encoding.ASCII.GetBytes("movej(p[0.10, 0.3, 0.4, 2.22, -2.22, 0.00], a=1.0, v=1)");
                    byte[] msg1 = Encoding.ASCII.GetBytes("set_digital_out(2,False)" + "\n");
                    // Send the data through the socket.
                    int bytesSent0 = sender.Send(msg2);
                    int bytesSent1 = sender.Send(msg1);
                    while (sender.Connected)
                    {
                        // Receive the response from the remote device.
                        int bytesRec = sender.Receive(bytes);
                        byte[] msgSize = { bytes[3], bytes[2], bytes[1], bytes[0] };
                        byte[] qt0 = { bytes[19], bytes[18], bytes[17], bytes[16], bytes[15], bytes[14], bytes[13], bytes[12] };
                        Array.Reverse(bytes);
                        //Console.WriteLine("Echoed test = {0}",
                        //Encoding.ASCII.GetString(bytes, 1020, 3));
                        var tic = BitConverter.ToInt32(msgSize, 0);
                        var tici = BitConverter.ToDouble(qt0, 0);
                        var q0 = BitConverter.ToDouble(bytes, 1096);
                        var q1 = BitConverter.ToDouble(bytes, 1088);
                        var q2 = BitConverter.ToDouble(bytes, 1080);
                        var q3 = BitConverter.ToDouble(bytes, 1072);
                        var q4 = BitConverter.ToDouble(bytes, 1064);
                        var q5 = BitConverter.ToDouble(bytes, 1056);

                        RobotPose[0] = q0;
                        RobotPose[1] = q1;
                        RobotPose[2] = q2;
                        RobotPose[3] = q3;
                        RobotPose[4] = q4;
                        RobotPose[5] = q5;

                        Console.WriteLine(tic);
                        Console.WriteLine($"qt0: {tici}");
                        Console.WriteLine($"q0: {q0}");
                        Console.WriteLine($"q1: {q1}");
                        Console.WriteLine($"q2: {q2}");
                        Console.WriteLine($"q3: {q3}");
                        Console.WriteLine($"q4: {q4}");
                        Console.WriteLine($"q5: {q5}");
                        Task.Delay(200);
                    }

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

}
