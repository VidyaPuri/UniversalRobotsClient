using RobotClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

namespace RobotClient.Networking
{
    public class SocketServer
    {
        #region Private Members

        private Dictionary<int, StateObject> clients = new Dictionary<int, StateObject>();
        private Socket listener;
        private bool isListening;

        #endregion

        public SocketServer()
        {

        }

        public static ManualResetEvent mre = new ManualResetEvent(false);

        public void StartListening(int port)
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[2];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            
            Debug.WriteLine("Server is listening for incomming connections.");

            try
            {
                // Create a TCP/IP socket.  
                using (listener)
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);

                    isListening = true;

                    while (isListening)
                    {
                        // Set the event to nonsignaled state.  
                        mre.Reset();
                        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                        // Wait until a connection is made before continuing.  
                        mre.WaitOne();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            // Bind the socket to the local endpoint and listen for incoming connections.  
                
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            mre.Set();
            if (isListening == false) return;
            StateObject state = new StateObject();
            try
            {
                lock (clients)
                {
                    state.Id = !clients.Any() ? 1 : clients.Keys.Max() + 1;
                    clients.Add(state.Id, state);
                    Debug.WriteLine("Client connected. Get Id " + state.Id);
                }

                state.listener = (Socket)ar.AsyncState;
                state.listener = state.listener.EndAccept(ar);
                state.listener.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //// Get the socket that handles the client request.  
            //Socket listener = (Socket)ar.AsyncState;
            //Socket handler = listener.EndAccept(ar);

            //// Create the state object.  
            ////StateObject state = new StateObject();


            //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            //    new AsyncCallback(ReadCallback), state);
         }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;

                    if (state.listener.Connected == false)
                        return;

                Socket handler = state.listener;

                // Read data from the client socket.
                int bytesRead = handler.EndReceive(ar);
                //int id = ar.AsyncState.Id as StateObject;
                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));
                    // Check for end-of-file tag. If it is not there, read
                    // more data.  
                    content = state.sb.ToString();
                    Debug.WriteLine($"Client {state.Id} is sending {content}");
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // All the data has been read from the
                        // client. Display it on the console.  
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content);
                        // Echo the data back to the client.  
                        Send(handler, content);
                    }
                    else
                    {
                        // Not all data received. Get more.  
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }

                    // Empty  the string buffer
                    state.sb.Clear();
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Close the Server Listener and all the open socket connections
        /// </summary>
        public void CloseServer()
        {
            if (listener != null)
            {
                isListening = false;
                ////listener.Shutdown(SocketShutdown.Both);
                listener.Close();
                //for (int i = clients.Count; i == 1; i--)
                while (clients.Count > 0)
                {
                    Close(clients.Count);
                }
                clients.Clear();
                Debug.WriteLine("Socket Server Listener is closed.");
            }
        }


        /// <summary>
        /// Gets the Client state object @ ID 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private StateObject GetClient(int id)
        {
            StateObject state = new StateObject();
            return clients.TryGetValue(id, out state) ? state : null;
        }

        /// <summary>
        /// Close the socket connection @ ID
        /// </summary>
        /// <param name="id"></param>
        public void Close(int id)
        {
            StateObject state = GetClient(id);
            if (state == null)
                Debug.WriteLine("Client does not exist.");
            else
            {
                try
                {
                    state.listener.Shutdown(SocketShutdown.Both);
                    state.listener.Close();
                }
                catch (SocketException)
                {
                    // TODO:
                }
                finally
                {
                    lock (clients)
                    {
                        clients.Remove(state.Id);
                        Console.WriteLine("Client disconnected with Id {0}", state.Id);
                    }
                }
            }
        }
    }
}
