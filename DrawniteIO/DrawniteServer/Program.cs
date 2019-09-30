using DrawniteCore.Networking;
using DrawniteCore.Networking.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrawniteServer
{
    class Program
    {
        private static LobbyManager LobbyManager;

        static async Task Main(string[] args)
        {
            TcpServerWrapper listeningService = new TcpServerWrapper(new IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), Constants.AUTH_PORT));
            LobbyManager.Init();
            LobbyManager = LobbyManager.Instance;
            listeningService.OnClientDataReceived += OnReceived;
            listeningService.OnClientConnected += ListeningService_OnClientConnected;
            listeningService.OnClientDisconnected += ListeningService_OnClientDisconnected;
            await listeningService.StartAsync();
            while (true)
            {
                LobbyManager?.Update();
            }
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
            Message networkMessage = (Message)args;
            switch (networkMessage.Command)
            {
                case "host":
                    Console.WriteLine("start host");
                break;

                case "join":
                    Console.WriteLine("validate and join");
                break;
            }
        }
    }
}
