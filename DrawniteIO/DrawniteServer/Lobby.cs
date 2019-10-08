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

        private Queue<Tuple<IConnection, Message>> playerMessageQueue;
        private TcpServerWrapper lobbyServer;
        private List<Player> playerList;
        private bool lobbyActive;
        private object runtimeLock;

        public int PlayerCount => lobbyServer.Connections.Count();

        public Lobby(Guid lobbyLeader, int lobbyPort)
        {
            this.lobbyInfo = new DrawniteCore.Networking.Data.LobbyInfo(Guid.NewGuid(), lobbyPort, lobbyLeader);
            this.lobbyServer = new TcpServerWrapper(new System.Net.IPEndPoint(IPAddress.Any, lobbyPort));
            this.lobbyServer.OnClientConnected += OnPlayerJoinedLobby;
            this.lobbyServer.OnClientDataReceived += OnPlayerDataReceived;
            this.lobbyServer.OnClientDisconnected += OnPlayerDisconnected;
            this.playerMessageQueue = new Queue<Tuple<IConnection, Message>>();
            this.playerList = new List<Player>();
            lobbyActive = this.lobbyServer.StartAsync().Result;

            this.runtimeLock = new object();
            if (lobbyActive)
                new Thread(RunGame).Start();
        }

        private void RunGame()
        {
            while (lobbyActive)
            {
                lock (runtimeLock)
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
        }

        public void Close()
        {
            _ = this.lobbyServer.ShutdownAsync();
        }

        private void OnPlayerJoinedLobby(IConnection client, dynamic args)
        {
            
        }

        private void OnPlayerDisconnected(IConnection client, dynamic args)
        {
            lock (runtimeLock)
            {
                Player disconnectingPlayer = playerList.Where(x => x.ReplicatedConnection == client).FirstOrDefault();
                if (disconnectingPlayer != null)
                {
                    bool retargetHost = false;
                    if (disconnectingPlayer.PlayerId == LobbyInfo.LobbyLeader)
                    {
                        if (playerList.Count - 1 == 0)
                            this.Close();
                        else
                            retargetHost = true;
                    }

                    playerList.Remove(disconnectingPlayer);

                    if (retargetHost)
                    {
                        this.lobbyInfo.LobbyLeader = playerList[0].PlayerId;
                        playerList[0].IsLeader = true;
                    }

                    playerList.ForEach(x => x.ReplicatedConnection.Write(new Message("player/disconnected", new
                    {
                        Player = disconnectingPlayer
                    })));
                }
            }
        }

        private void OnPlayerDataReceived(IConnection client, dynamic args)
        {
            playerMessageQueue.Enqueue(new Tuple<IConnection, Message>(client, (Message)args));
        }

        private void HandleMessage(Tuple<IConnection, Message> message)
        {
            switch (message.Item2.Command)
            {
                case "player/join":
                {
                    Guid playerGuid = message.Item2.Data.GUID;
                    string playerName = message.Item2.Data.Username;
                    Player player = new Player(playerGuid, playerName, playerGuid == lobbyInfo.LobbyLeader);
                    player.ReplicatedConnection = message.Item1;
                    playerList.Add(player);
                    Message networkMessage = new Message("player/connected", new
                    {
                        Player = player
                    });

                    for (int i = 0; i < lobbyServer.Connections.Count(); i++)
                    {
                        lobbyServer.Connections.ElementAt(i).Write(networkMessage);
                    }
                }
                break;

                case "lobby/playerlist":
                {
                    Message networkMessage = new Message("lobby/playerlist", new
                    {
                        PlayerList = playerList,
                    });
                    message.Item1.Write(networkMessage);
                }
                break;

                case "lobby/cancel":
                {
                    this.lobbyInfo.LobbyStatus = LobbyStatus.AWAITING_START;
                    Message networkMessage = new Message("lobby/cancelled", null);
                    playerList.ForEach(x => x.ReplicatedConnection.Write(networkMessage));
                }
                break;

                case "lobby/trystart":
                {
                    Guid sender = message.Item2.Data.PlayerId;
                    if (sender != lobbyInfo.LobbyLeader)
                    {
                        message.Item1.Write(new Message("lobby/error", new
                        {
                            ErrorMessage = "You are not the host",
                        }));
                    }
                    else
                    {
                        if (playerList.Count >= 2)
                        {
                            lobbyInfo.LobbyStatus = LobbyStatus.STARTING;
                            playerList.ForEach(x => x.ReplicatedConnection.Write(new Message("lobby/starting", null)));
                        }
                        else
                        {
                            message.Item1.Write(new Message("lobby/error", new
                            {
                                ErrorMessage = "Not enough players to start the game. Need 2 or more players.",
                            }));
                        }
                    }
                }
                break;

                case "lobby/start":
                {
                    playerList.ForEach(x => x.ReplicatedConnection.Write(new Message("game/start", null)));
                }
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
