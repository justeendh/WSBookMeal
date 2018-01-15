using SocketCommunicate;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketCommunicate
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 10240;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
    }

    public class AsynchronousSocketListener
    {
        public ManualResetEvent allDone = new ManualResetEvent(false);
        private IServerProccessing _processing;
        public const int BUFFER_SIZE = 10240;
        public const int MAX_CLIENTS = 100;

        Socket listener;
        IPEndPoint localEndPoint;
        private Dictionary<string, Socket> Clients;
        string AppName;


        public AsynchronousSocketListener(int PORT_LISTEN, IServerProccessing processing, string ApplicationName)
        {
            Random rnd = new Random();
            AppName = string.IsNullOrEmpty(ApplicationName) ? string.Format("ID_{0:0000}", rnd.Next(1, int.MaxValue)) : ApplicationName;
            Clients = new Dictionary<string, Socket>();
            _processing = processing;
            byte[] bytes = new Byte[BUFFER_SIZE];
            localEndPoint = new IPEndPoint(IPAddress.Any, PORT_LISTEN);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Thread threadCheckClientConnection = new Thread(() =>
            {
                lock (Clients)
                {
                    if (Clients != null && Clients.Count > 0)
                    {
                        List<string> lstRemove = new List<string>();
                        foreach (var client in Clients)
                        {
                            Socket clientProccess = client.Value;
                            if (!SocketExtensions.IsConnected(clientProccess))
                            {
                                IPEndPoint localIpEndPoint = clientProccess.LocalEndPoint as IPEndPoint;
                                if (Clients.ContainsKey(localIpEndPoint.ToString())) Clients.Remove(localIpEndPoint.Address.ToString());
                                Console.WriteLine("Disconected to {0}", localIpEndPoint.ToString());
                                clientProccess.Shutdown(SocketShutdown.Both);
                                clientProccess.Close();
                                lstRemove.Add(client.Key);
                            }
                        }

                        foreach (var client in lstRemove)
                        {
                            Clients.Remove(client);
                        }
                    }
                }
            });
            threadCheckClientConnection.IsBackground = true;
            threadCheckClientConnection.Start();
        }

        public void Start()
        {
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(MAX_CLIENTS);
                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("{0} Waiting for a connection...", AppName);
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                //EventsLogging.WriteLogError(ex);
                //Console.WriteLine(ex.ToString());
                Environment.Exit(0);
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }
        
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            IPEndPoint localIpEndPoint = handler.LocalEndPoint as IPEndPoint;
            Console.WriteLine("Client connected: {0}", handler.RemoteEndPoint.ToString());

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            if(!Clients.ContainsKey(localIpEndPoint.Address.ToString())) Clients.Add(localIpEndPoint.Address.ToString(), handler);
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    byte[] byteRecv = new byte[bytesRead];
                    Array.Copy(state.buffer, 0, byteRecv, 0, bytesRead);
                    byte[] dataReply = null;
                    bool Success = false;
                    string KeyDataInStr = Encoding.Unicode.GetString(byteRecv);
                    if(_processing != null) Success = _processing.ActionProcessingData(KeyDataInStr);
                    if (Success) dataReply = Encoding.Unicode.GetBytes("SUCCESS");
                    else dataReply = Encoding.Unicode.GetBytes("ERROR");
                    Send(handler, dataReply);
                    StateObject new_state = new StateObject();
                    new_state.workSocket = handler;
                    handler.BeginReceive(new_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), new_state);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconected to {0}", handler.RemoteEndPoint.ToString());
                IPEndPoint localIpEndPoint = handler.LocalEndPoint as IPEndPoint;
                if (Clients.ContainsKey(localIpEndPoint.ToString())) Clients.Remove(localIpEndPoint.Address.ToString());
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }
        }

        public void Send(Socket handler, byte[] data)
        {            
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
