using AsynchronousSockeClient.Networking;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using tcpClientWPF.ViewModels;


namespace AsynchronousSocketClient.Networking
{
    public class ClientSocket
    {
        private Socket _socket;
        private byte[] _buffer;

        ShellViewModel svm = new ShellViewModel();

        public ClientSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ipAddress, int port)
        {
            if(!_socket.Connected)
            {
                _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectCallback, null);
                Console.WriteLine("Connected to the server!");
            }
            else
            {
                Debug.WriteLine("Already connected to the server!");
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            if (_socket.Connected)
            {
                StartReceiving();
            }
            else
                Console.WriteLine("Could not connect!");
        }

        private void ReceivedCallback(IAsyncResult result)
        {
            int buffLength = _socket.EndReceive(result);
            byte[] packet = new byte[buffLength];

            Array.Copy(_buffer, packet, packet.Length);

            // Read header for getting message size - TODO: proper implementation of this
            //int pacSize = BitConverter.ToInt32(new byte[] {packet[3], packet[2], packet[1], packet[0] }, 0);
            //byte[] pacBuffer = new byte[pacSize];
            //_socket.Receive(pacBuffer, pacSize, SocketFlags.None);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(packet);

            // Handle packet
            
            double[] rp = PacketHandler.Handle(packet, packet.Length);

            Application.Current.Dispatcher.Invoke(() => svm.UpdateUI(rp));
            //StartReceiving();
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public void StartReceiving()
        {
            try
            {
                _buffer = new byte[1116];
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceivedCallback, null);
            }
            catch { }
        }

        public void Send(byte[] data)
        {
            _socket.Send(data);
        }
    }
}
