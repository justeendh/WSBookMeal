using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketCommunicate
{
    public delegate void SendActionCompleated();
    public class AsynchronousClient
    {
        Socket client;
        private const int TIME_OUT = 5000;

        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private byte[] response;
        private bool IsReady;

        public event SendActionCompleated OnSendActionCompleated;

        public bool StartClient(string Address, int Port)
        {
            IsReady = false;
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(Address, Port, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne(TIME_OUT);
                if (client.Connected)
                {
                    IsReady = true;
                    return true;
                }
                else
                {
                    IsReady = false;
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                IsReady = false;
                return false;
            }
        }
        
        public void SendData(string KeyHoaDon)
        {
            if (IsReady && client != null && client.Connected)
            {
                byte[] SendDataa =  Encoding.Unicode.GetBytes(KeyHoaDon);
                sendDone.Reset();
                receiveDone.Reset();
                Send(client, SendDataa);
                sendDone.WaitOne(5000);
                if (OnSendActionCompleated != null) OnSendActionCompleated();
                return;
            }
            return;
        }

        public void Close()
        {
            try
            {
               client.Shutdown(SocketShutdown.Both);
               client.Close();
            }
            catch (Exception ex)
            {
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                Console.WriteLine("{0:dd-MM-yyyy HH:mm:ss}: Connected to {1}", DateTime.Now, client.RemoteEndPoint.ToString());
                connectDone.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                try
                {
                    int bytesRead = client.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        byte[] byteRecv = new byte[bytesRead];
                        Array.Copy(state.buffer, 0, byteRecv, 0, bytesRead);                       
                        response = byteRecv;
                        
                        receiveDone.Set();
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Disconected to {0}", client.RemoteEndPoint.ToString());
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Send(Socket client, byte[] data)
        {
            client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {  
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
