using Newtonsoft.Json;
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
    public class NetworkConnection : IConnection
    {
        private readonly NetworkStream networkStream;
        private readonly object networkLock;

        public Data.Message LastMessage { get; private set; }

        private bool active = false;

        private readonly IPEndPoint remoteEndPoint;
        public IPEndPoint RemoteEndPoint => remoteEndPoint;

        public event IConnection.ConnectionEventHandler OnReceived;
        public event IConnection.ConnectionEventHandler OnSuccessfulConnection;
        public event IConnection.ConnectionEventHandler OnDisconnected;
        public event IConnection.ConnectionEventHandler OnError;
        private object writeLock = new object();

        public NetworkConnection(ref NetworkStream networkStream, IPEndPoint remoteEndPoint)
        {
            this.networkLock = new object();
            this.networkStream = networkStream;
            active = true;
            new Thread(Receive).Start();
            this.remoteEndPoint = remoteEndPoint;
            OnSuccessfulConnection?.Invoke(this, null);
        }

        public void Write(string data)
        {
            this.Write(Encoding.ASCII.GetBytes(data));
        }

        public void Write(dynamic json)
        {
            this.Write(JsonConvert.SerializeObject(json));
        }

        public void Write(byte[] data)
        {
            lock (writeLock)
            {
                byte[] messageLength = BitConverter.GetBytes(data.Length);
                networkStream.Write(messageLength, 0, messageLength.Length);
                networkStream.Write(data, 0, data.Length);
            }
        }

        private void Receive()
        {
            try
            {
                while (active)
                {
                    byte[] lengthBuffer = new byte[4];
                    networkStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    int receivingByteSize = BitConverter.ToInt32(lengthBuffer, 0);

                    if (receivingByteSize <= 0)
                        break;

                    byte[] networkMessage = new byte[receivingByteSize];
                    networkStream.Read(networkMessage, 0, networkMessage.Length);

                    try
                    {
                        Data.Message receivedMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.Message>(Encoding.ASCII.GetString(networkMessage));

                        LastMessage = receivedMessage;
                        OnReceived?.Invoke(this, receivedMessage);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, e);
            }
            finally
            {
                OnDisconnected?.Invoke(this, null);
            }
        }

        private void WriteConfirmation()
        {
            byte[] receiving = new byte[] { 0 };
            networkStream.Write(receiving, 0, receiving.Length);
        }

        private void AwaitConfirmation()
        {
            byte[] msg = new byte[1];
            networkStream.Read(msg, 0, msg.Length);
        }

        public void Shutdown()
        {
            if (active)
                active = false;
        }

        public void Dispose()
        {
            
        }
    }
}
