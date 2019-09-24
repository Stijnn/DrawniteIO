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
    public sealed class ListeningService
    {
        private TcpListener listener;
        private CancellationTokenSource runtimeToken;
        private IList<ClientServer> connectedClients;

        public ListeningService(IPEndPoint endPoint)
        {
            this.listener = new TcpListener(endPoint.Address, endPoint.Port);
            this.connectedClients = new List<ClientServer>();
        }

        public async Task StartAsync()
        {
            if (runtimeToken == null)
            {
                runtimeToken = new CancellationTokenSource();
                Task.Run(AcceptClients);
            }
        }

        public async Task ShutdownAsync()
        {
            if (runtimeToken != null)
            {
                runtimeToken.Cancel();
                for (int i = 0; i < connectedClients.Count; i++)
                    connectedClients[i].Shutdown();
                runtimeToken = null;
            }
        }

        private async Task AcceptClients()
        {
            listener.Start();
            while (!runtimeToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                ClientServer clientServer = new ClientServer(ref client);
                connectedClients.Add(clientServer);
                Console.WriteLine($"CLIENT CONNECTED: {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}");
                clientServer.OnDataReceived += OnDataReceived;
            }
            listener.Stop();
        }

        private void OnDataReceived(ClientServer sender, object args)
        {
            Console.WriteLine(Encoding.ASCII.GetString((byte[])args));
        }

        public void Broadcast(byte[] data)
        {
            for (int i = 0; i < connectedClients.Count; i++)
            {
                connectedClients[i].SendData(data);
            }
        }
    }
}
