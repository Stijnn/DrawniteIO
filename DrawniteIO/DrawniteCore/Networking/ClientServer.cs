using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawniteCore.Networking
{
    public class ClientServer
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream => tcpClient.GetStream();
        private bool shuttingDown = false;
        private object networkLock = new object();

        public delegate void NetworkEventHandler(ClientServer sender, object args);
        public event NetworkEventHandler OnDataReceived;
        public event NetworkEventHandler OnClientDisconnected;
        public event NetworkEventHandler OnClientError;

        public ClientServer(ref TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            new Thread(Routine).Start();
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
                while (!shuttingDown)
                {
                    lock (networkLock)
                    {
                        if (tcpClient.Available > 0)
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

        public void Shutdown()
        {
            if (!shuttingDown)
                shuttingDown = true;
        }
    }
}
