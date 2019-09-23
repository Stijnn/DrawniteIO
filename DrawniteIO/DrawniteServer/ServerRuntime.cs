using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawniteServer
{
    public sealed class ServerRuntime
    {
        private TcpListener listener;
        private List<TcpClient> connectedClients;
        private bool running = false;

        public ServerRuntime(IPEndPoint endPoint)
        {
            listener = new TcpListener(endPoint);
            connectedClients = new List<TcpClient>();
        }

        public void Start()
        {
            if (!running)
            {
                running = true;
                listener.Start();
                new Thread(async () =>
                {
                    while (running)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        connectedClients.Add(client);

                        new Thread(async () =>
                        {
                            while (running)
                            {
                                int bytesAvailable = client.Available;
                                if (bytesAvailable > 0)
                                {
                                    byte[] buffer = new byte[bytesAvailable];
                                    client.GetStream().Read(buffer, 0, buffer.Length);

                                    string content = Encoding.UTF8.GetString(buffer);
                                    Console.WriteLine(content);
                                    if (content == "PING")
                                    {
                                        byte[] sending = Encoding.UTF8.GetBytes("PONG");
                                        client.GetStream().Write(sending, 0, sending.Length);
                                        Console.WriteLine("PONG");
                                    }
                                }
                            }
                        }).Start();
                    }
                }).Start();
            }
        }
    }
}
