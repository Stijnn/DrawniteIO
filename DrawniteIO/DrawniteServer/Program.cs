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
        private static TcpServerWrapper listeningService = new TcpServerWrapper(new IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), Constants.AUTH_PORT));

        static async Task Main(string[] args)
        {
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
                    {
                        Lobby lobby = LobbyManager.Instance.NewLobby(Guid.NewGuid());
                        client.Write(new Message("lobby/create", new
                        {
                            LobbyInfo = lobby.LobbyInfo,
                            PlayerGuid = lobby.LobbyInfo.LobbyLeader
                        }));
                    }
                break;

                case "join":
                    {
                        Guid lobbyId = networkMessage.Data.LobbyId;
                        Lobby lobby = LobbyManager.Instance.FindLobby(lobbyId);
                        client.Write(new Message("lobby/join", new
                        {
                            LobbyInfo = lobby.LobbyInfo,
                            PlayerGuid = Guid.NewGuid()
                        }));
                    }
                break;
            }
        }
    }
}
