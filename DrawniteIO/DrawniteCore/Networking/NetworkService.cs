using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawniteCore.Networking
{
    public sealed class NetworkService
    {
        private TcpClient client;
        private object networkLock = new object();
        private NetworkStream networkStream => client.GetStream();
        private bool running = false;

        public delegate void NetworkEventHandler(NetworkService service, object args);
        public event NetworkEventHandler OnDataReceived;
        public event NetworkEventHandler OnClientDisconnected;
        public event NetworkEventHandler OnClientError;

        public NetworkService()
        {
            this.client = new TcpClient();
        }

        public void Connect(IPEndPoint endPoint)
        {
            if (!client.Connected)
            {
                client.Connect(endPoint);
                running = true;
                new Thread(Routine).Start();
            }
        }

        public void SendData(byte[] data)
        {
            lock (networkLock)
            {
                byte[] messageLength = BitConverter.GetBytes(data.Length);
                networkStream.Write(messageLength, 0, messageLength.Length);
                AwaitConfirmation();
                WriteConfirmation();
                networkStream.Write(data, 0, data.Length);
                AwaitConfirmation();
                WriteConfirmation();
            }
        }

        private void Routine()
        {
            try
            {
                while (running)
                {
                    if (client.Available > 0)
                    {
                        lock (networkLock)
                        {
                            byte[] lengthBuffer = new byte[4];
                            networkStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                            int receivingByteSize = BitConverter.ToInt32(lengthBuffer, 0);

                            if (receivingByteSize <= 0)
                            {
                                break;
                            }
                            WriteConfirmation();
                            AwaitConfirmation();

                            byte[] networkMessage = new byte[receivingByteSize];
                            networkStream.Read(networkMessage, 0, networkMessage.Length);
                            WriteConfirmation();
                            AwaitConfirmation();

                            OnDataReceived?.Invoke(this, networkMessage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                OnClientError?.Invoke(this, e);
            }
            finally
            {
                OnClientDisconnected?.Invoke(this, null);
            }
        }

        private void WriteConfirmation()
        {
            byte[] receiving = Encoding.ASCII.GetBytes("RECV");
            networkStream.Write(receiving, 0, receiving.Length);
        }

        private void AwaitConfirmation()
        {
            while (!networkStream.DataAvailable)
                Thread.Sleep(5);
            byte[] confirmationMessage = Encoding.ASCII.GetBytes("RECV");
            byte[] msg = new byte[confirmationMessage.Length];
            networkStream.Read(msg, 0, msg.Length);
            Console.WriteLine(Encoding.ASCII.GetString(msg));
        }

        public void Disconnect()
        {
            if (client.Connected)
            {
                running = false;
                client.Close();
                client = new TcpClient();
            }
        }
    }
}
