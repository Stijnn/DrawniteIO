using DrawniteCore.Networking;
using DrawniteCore.Networking.Data;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string GetRandomWord()
        {
            string[] JSON_FOLDER_SEGEMENTS = new Uri(Directory.GetCurrentDirectory()).Segments;
            string JSON_FOLDER_NOUSAGE = string.Concat(Util.SubArray(JSON_FOLDER_SEGEMENTS, 0, JSON_FOLDER_SEGEMENTS.Length - 3)) + "words.txt";
            string wordsFile = JSON_FOLDER_NOUSAGE.Remove(0, 1);
            string[] words = File.ReadAllLines(wordsFile);
            Random random = new Random(DateTime.Now.Millisecond);
            return words[random.Next(0, words.Length - 1)];
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
                        lobbyInfo.LobbyStatus = LobbyStatus.PLAYING;
                    }
                break;

                case "game/selected":
                {
                    gameState = GameState.PLAYING;
                }
                break;

                case "canvas/update":
                {
                    if (selectedPlayer.ReplicatedConnection == message.Item1)
                    {
                        playerList.ForEach(x =>
                        {
                            if (x.ReplicatedConnection != message.Item1)
                                x.ReplicatedConnection.Write(message.Item2);
                        });
                    }
                }
                break;

                case "game/chat":
                    {
                        Player sender = playerList.Where(x => x.ReplicatedConnection == message.Item1).FirstOrDefault();
                        if (scoreBoard != null && sender != null)
                        {
                            if (guessers.Contains(sender))
                            {
                                message.Item2.Data.Message = $"{sender.Username}: " + message.Item2.Data.Message;
                                for (int i = 0; i < guessers.Count; i++)
                                {
                                    guessers.ElementAt(i).ReplicatedConnection.Write(message.Item2);
                                }
                            }
                            else if (!guessers.Contains(sender) && message.Item2.Data.Message == selectedWord)
                            {
                                guessers.Push(sender);
                            }
                            else
                            {
                                message.Item2.Data.Message = $"{sender.Username}: " + message.Item2.Data.Message;
                                playerList.ForEach(x => x.ReplicatedConnection.Write(message.Item2));
                            }
                        }
                    }
                break;
            }
        }

        private void AwaitingStart()
        {

        }

        enum GameState
        {
            LOADING,
            SELECTING,
            AWAITING,
            PLAYING,
            SCORE
        }

        private GameState gameState = GameState.LOADING;
        private Player selectedPlayer;
        private int playerIndexOffset;
        private int rounds = 1;
        private int currentRound = 0;
        private string selectedWord;
        private int secondsRemaining = 120;
        private long startTime = 0;
        private long currentTime = 0;

        private Dictionary<string, int> scoreBoard;
        private Stack<Player> guessers;
        private void Play()
        {
            switch (gameState)
            {
                case GameState.LOADING:
                    selectedPlayer = playerList[playerIndexOffset++];
                    gameState = GameState.SELECTING;
                    secondsRemaining = 120;
                    guessers = new Stack<Player>();

                    if (scoreBoard == null)
                    {
                        scoreBoard = new Dictionary<string, int>();
                        playerList.ForEach(x => scoreBoard.Add(x.Username, 0));
                    }
                break;

                case GameState.SELECTING:
                    selectedWord = GetRandomWord();
                    selectedPlayer.ReplicatedConnection.Write(new Message("game/selected", new
                    {
                        Word = selectedWord,
                    }));

                    playerList.ForEach(x =>
                    {
                        if (x != selectedPlayer)
                            x.ReplicatedConnection.Write(new Message("game/awaiting", new
                            {
                                Drawer = selectedPlayer.Username,
                                PlayerList = playerList,
                            }));
                    });

                    gameState = GameState.AWAITING;
                break;

                case GameState.PLAYING:
                    if (startTime == 0)
                    {
                        playerList.ForEach(x =>
                        {
                            if (x != selectedPlayer)
                                x.ReplicatedConnection.Write(new Message("game/starting", new
                                {
                                    Drawer = selectedPlayer.Username,
                                    PlayerList = playerList,
                                }));
                        });

                        startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                    else
                        currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (currentTime - startTime >= 1000)
                    {
                        secondsRemaining--;
                        playerList.ForEach(x =>
                        {
                            x.ReplicatedConnection.Write(new Message("game/playing", new
                            {
                                TimeLeft = secondsRemaining,
                                PlayerList = playerList,
                                ScoreBoard = scoreBoard
                            }));
                        });

                        startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }

                    if (guessers.Count == playerList.Count - 1)
                        secondsRemaining = 0;

                    if (secondsRemaining == 0)
                    {
                        int index = 0;
                        int baseScore = 10;
                        while(guessers.Count > 0)
                        {
                            Player guesser = guessers.Pop();
                            scoreBoard[guesser.Username] = baseScore + index++;
                        }

                        if (playerIndexOffset == playerList.Count)
                            playerIndexOffset = 0;

                        if (selectedPlayer == playerList[playerList.Count - 1])
                            currentRound++;

                        if (currentRound == rounds)
                            gameState = GameState.SCORE;
                        else
                            gameState = GameState.LOADING;
                    }
                break;

                case GameState.SCORE:
                    playerList.ForEach(x =>
                    {
                        x.ReplicatedConnection.Write(new Message("game/end", new
                        {
                            Scoreboard = scoreBoard
                        }));
                    });

                    startTime = 0;
                    currentRound = 0;
                    gameState = GameState.AWAITING;
                    LobbyInfo.LobbyStatus = LobbyStatus.AWAITING_RESTART;
                break;
            }

            Thread.Sleep(5);
        }

        private void AwaitingRestart()
        {
            gameState = GameState.LOADING;
            LobbyInfo.LobbyStatus = LobbyStatus.AWAITING_START;
        }
    }
}
