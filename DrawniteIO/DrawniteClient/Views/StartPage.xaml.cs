using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    using DrawniteCore.Networking;
    using DrawniteCore.Networking.Data;
    using MahApps.Metro.Controls.Dialogs;
    using Newtonsoft.Json;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : NetworkPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private async void OnConnectBttn(object sender, RoutedEventArgs e)
        {
            string username = await (Application.Current.MainWindow as MainWindow).ShowInputAsync("Connect", "You clicked connect");
            this.NetworkConnection.Write(new DrawniteCore.Networking.Data.Message("player/join", new
            {
                LobbyId = Guid.Parse(txtLobbyId.Text)
            }));

            DrawniteCore.Networking.Data.Message returnMessage = null;
            ManualResetEvent receivedSignal = new ManualResetEvent(false);
            this.NetworkConnection.OnReceived += async (client, message) =>
            {
                returnMessage = message;
                receivedSignal.Set();
            };
            receivedSignal.WaitOne();

            int lobbyPort = returnMessage.Data.LobbyInfo.LobbyPort;
            bool result = this.NetworkClient.Forward(new System.Net.IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), lobbyPort));
            if (result)
            {
                this.NetworkConnection.Write(new Message("player/join", new
                {
                    Username = username,
                    GUID = returnMessage.Data.PlayerGuid
                }));
            }

            LobbyStatus gameState = returnMessage.Data.LobbyInfo.LobbyStatus;
            Guid lobbyId = returnMessage.Data.LobbyInfo.LobbyId;
            Guid playerId = returnMessage.Data.PlayerGuid;
            Guid lobbyLeader = returnMessage.Data.LobbyInfo.LobbyLeader;

            switch (gameState)
            {
                case LobbyStatus.AWAITING_START:
                    this.NavigationService.Navigate(new LobbyPage(lobbyId, playerId, playerId == lobbyLeader));
                break;

                case LobbyStatus.PLAYING:
                    this.NavigationService.Navigate(new GamePage(lobbyId, playerId));
                break;

                case LobbyStatus.AWAITING_RESTART:

                break;

                default:
                    
                break;
            }
        }

        private async void OnHostBttn(object sender, RoutedEventArgs e)
        {
            LoginDialogData data = await (Application.Current.MainWindow as MainWindow).ShowLoginAsync("Host", "Enter username");
            this.NetworkConnection.Write(new DrawniteCore.Networking.Data.Message("player/host", new 
            { 
                Username = data.Username,
                Password = data.Password
            }));

            DrawniteCore.Networking.Data.Message returnMessage = null;
            ManualResetEvent receivedSignal = new ManualResetEvent(false);
            this.NetworkConnection.OnReceived += async (client, message) => 
            {
                returnMessage = message;
                receivedSignal.Set();
            };
            receivedSignal.WaitOne();

            int lobbyPort = returnMessage.Data.LobbyInfo.LobbyPort;
            bool result = this.NetworkClient.Forward(new System.Net.IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), lobbyPort));
            if (result)
            {
                this.NetworkConnection.Write(new Message("player/join", new
                {
                    Username = data.Username,
                    GUID = returnMessage.Data.PlayerGuid
                }));
            }

            Guid lobbyId = returnMessage.Data.LobbyInfo.LobbyId;
            Guid playerId = returnMessage.Data.PlayerGuid;
            Guid lobbyLeader = returnMessage.Data.LobbyInfo.LobbyLeader;
            this.NavigationService.Navigate(new LobbyPage(lobbyId, playerId, playerId == lobbyLeader));
        }
    }
}
