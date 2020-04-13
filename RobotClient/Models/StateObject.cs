using System.Net.Sockets;
using System.Text;

namespace RobotClient.Models
{
    public class StateObject
    {
        // Id 
        private int id;
        // Client  socket.  
        public Socket listener = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }
    }
}
