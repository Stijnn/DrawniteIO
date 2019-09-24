using DrawniteCore.Networking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrawniteServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TcpServerWrapper listeningService = new TcpServerWrapper(new IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), Constants.AUTH_PORT));
            listeningService.OnClientDataReceived += OnReceived;
            listeningService.OnClientConnected += ListeningService_OnClientConnected;
            listeningService.OnClientDisconnected += ListeningService_OnClientDisconnected;
            await listeningService.StartAsync();
            while (true)
            {
                string command = Console.ReadLine();
                if (command == "shutdown")
                    break;
                else
                {
                    new List<IConnection>(listeningService.Connections).ForEach(x => x.Write(command));
                }
            }
            await listeningService.ShutdownAsync();
        }

        private static void ListeningService_OnClientConnected(IConnection client, dynamic args)
        {
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} CONNECTED");
        }

        private static void ListeningService_OnClientDisconnected(IConnection client, dynamic args)
        {
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} DISCONNECTED");
        }

        private static void OnReceived(IConnection client, dynamic args)
        {
            Console.WriteLine($"CLIENT {client.RemoteEndPoint.Address} SENT {Encoding.ASCII.GetString(args)}");
        }
    }
}
