using DrawniteClient.Views.Controls;
using DrawniteCore.Networking.Data;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DrawniteClient.Views
{
    /// <summary>
    /// Interaction logic for LobbyPage.xaml
    /// </summary>
    public partial class LobbyPage : NetworkPage
    {
        private bool IsLobbyLeader;
        private Guid MyPlayerId;
        private Guid LobbyId;
        private ProgressDialogController controller;

        public LobbyPage(Guid lobbyId, Guid playerId, bool isLeader)
        {
            InitializeComponent();
            IsLobbyLeader = isLeader;
            this.MyPlayerId = playerId;
            this.LobbyId = lobbyId;
            this.NetworkConnection.OnReceived += OnPacketReceived;
            txtLobbyKey.Text = lobbyId.ToString();
            btnStart.IsEnabled = IsLobbyLeader;
            this.NetworkConnection.Write(new Message("lobby/playerlist", null));
        }

        private async void OnPacketReceived(DrawniteCore.Networking.IConnection connection, dynamic args)
        {
            Message packet = (Message)args;
            switch (packet.Command)
            {
                case "player/connected":
                {
                    Player player = ((JObject)args.Data.Player).ToObject<Player>();
                    Dispatcher.Invoke(() =>
                    {
                        lvConnectedPlayers.Children.Add(new Controls.ConnectedPlayer(player.Username, player.IsLeader));
                    });
                }
                break;

                case "player/disconnected":
                {
                    this.NetworkConnection.Write(new Message("lobby/playerlist", null));
                }
                break;

                case "lobby/playerlist":
                {
                    List<Player> playerList = ((JArray)packet.Data.PlayerList).ToObject<List<Player>>();
                    Dispatcher.Invoke(() =>
                    {
                        lvConnectedPlayers.Children.Clear();
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            lvConnectedPlayers.Children.Add(new Controls.ConnectedPlayer(playerList[i].Username, playerList[i].IsLeader));

                            if (playerList[i].PlayerId == MyPlayerId && playerList[i].IsLeader)
                                LeaderSwitched();
                        }
                    });
                }
                break;

                case "lobby/error":
                {
                    string errorMessage = packet.Data.ErrorMessage;
                    await Dispatcher.Invoke(async () =>
                    {
                        await ParentWindow.ShowMessageAsync("Error", errorMessage, MessageDialogStyle.Affirmative);
                    });
                }
                break;

                case "lobby/starting":
                {
                    await Dispatcher.Invoke(async () =>
                    {
                        controller = await ParentWindow.ShowProgressAsync("Starting", "The game is starting soon...", IsLobbyLeader);
                        controller.Maximum = 10010;
                        controller.Minimum = 0;
                    });

                    controller.Canceled += (x,y) =>
                    {
                        if (IsLobbyLeader)
                        {
                            NetworkConnection.Write(new Message("lobby/cancel", null));
                        }
                    };

                    Thread t = new Thread(async () =>
                    {
                        //Yes every 60 seconds in Africa a minute passes, together we can stop this.
                        long secondPassedCheck = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        long actualStart = secondPassedCheck;
                        int currentSecond = 0;
                        while (controller.IsOpen)
                        {
                            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            if (currentSecond == 10)
                                break;

                            if (currentTime - secondPassedCheck >= 1000)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    controller.SetTitle($"Starting in 00:{string.Format("{0:D2}", 10 - currentSecond++)}");
                                });
                                secondPassedCheck = currentTime;
                            }

                            long ms = currentTime - actualStart;
                            controller.SetProgress(ms);
                        }

                        if (currentSecond == 10)
                        {
                            await controller.CloseAsync();
                            if (IsLobbyLeader)
                            {
                                NetworkConnection.Write(new Message("lobby/start", new
                                {
                                    PlayerId = MyPlayerId,
                                }));
                            }
                        }
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                }
                break;

                case "lobby/cancelled":
                {
                    await Dispatcher.Invoke(async () =>
                    {
                        await controller?.CloseAsync();
                    });
                }
                break;

                case "game/start":
                {
                    if (controller.IsOpen)
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            await controller?.CloseAsync();
                        });
                    }

                    await Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        this.NavigationService.Navigate(new GamePage(LobbyId, MyPlayerId));
                    }), null);
                }
                break;
            }
        }

        private void LeaderSwitched()
        {
            IsLobbyLeader = true;
            btnStart.IsEnabled = IsLobbyLeader;
        }

        private void OnCopyKeyClicked(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtLobbyKey.Text);
        }

        private void OnStartGame(object sender, RoutedEventArgs e)
        {
            if (IsLobbyLeader)
            {
                NetworkConnection.Write(new Message("lobby/trystart", new
                {
                    PlayerId = MyPlayerId,
                }));
            }
        }
    }
}
