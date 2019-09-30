using DrawniteCore.Networking;
using DrawniteCore.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawniteServer
{
    class Lobby
    {
        private DrawniteCore.Networking.Data.LobbyInfo lobbyInfo;
        public DrawniteCore.Networking.Data.LobbyInfo LobbyInfo => lobbyInfo;

        private Queue<Message> playerMessageQueue;
        private TcpServerWrapper lobbyServer;
        private Dictionary<Guid, string> playerList;
        private bool lobbyActive;

        public int PlayerCount => lobbyServer.Connections.Count();

        public Lobby(Guid lobbyLeader, int lobbyPort)
        {
            this.lobbyInfo = new DrawniteCore.Networking.Data.LobbyInfo(Guid.NewGuid(), lobbyPort, lobbyLeader);
            this.lobbyServer = new TcpServerWrapper(new System.Net.IPEndPoint(IPAddress.Any, lobbyPort));
            this.lobbyServer.OnClientConnected += OnPlayerJoinedLobby;
            this.lobbyServer.OnClientDataReceived += OnPlayerDataReceived;
            this.playerMessageQueue = new Queue<Message>();
            this.playerList = new Dictionary<Guid, string>();
            lobbyActive = this.lobbyServer.StartAsync().Result;

            if (lobbyActive)
                new Thread(RunGame).Start();
        }

        private void RunGame()
        {
            while (lobbyActive)
            {
                if (playerMessageQueue.Count > 0)
                {
                    HandleMessage(playerMessageQueue.Dequeue());
                    continue;
                }

                switch (LobbyInfo.LobbyStatus)
                {
                    case DrawniteCore.Networking.Data.LobbyStatus.AWAITING_START:
                        AwaitingStart();
                    break;

                    case DrawniteCore.Networking.Data.LobbyStatus.PLAYING:
                        Play();
                    break;

                    case DrawniteCore.Networking.Data.LobbyStatus.AWAITING_RESTART:
                        AwaitingRestart();
                    break;
                }
            }
        }

        public void Close()
        {
            _ = this.lobbyServer.ShutdownAsync();
        }

        private void OnPlayerJoinedLobby(IConnection client, dynamic args)
        {
            
        }

        private void OnPlayerDataReceived(IConnection client, dynamic args)
        {
            playerMessageQueue.Enqueue(args);
        }

        private void HandleMessage(Message msg)
        {
            switch (msg.Command)
            {
                case "player/join":
                    Guid playerGuid = msg.Data.GUID;
                    string playerName = msg.Data.Username;
                    playerList.Add(playerGuid, playerName);
                break;

                default:
                break;
            }
        }

        private void AwaitingStart()
        {

        }

        private void Play()
        {

        }

        private void AwaitingRestart()
        {

        }
    }
}
