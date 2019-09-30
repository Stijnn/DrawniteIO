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

namespace DrawniteClient.Pages
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
    public partial class StartPage : Page
    {
        private Frame parentFrameView;

        public StartPage(ref Frame parentFrameView)
        {
            InitializeComponent();
            this.parentFrameView = parentFrameView;
        }

        private async void OnConnectBttn(object sender, RoutedEventArgs e)
        {
            string username = await (Application.Current.MainWindow as MainWindow).ShowInputAsync("Connect", "You clicked connect");
            (Application.Current as App).ClientWrapper.NetworkConnection.Write(new DrawniteCore.Networking.Data.Message("join", new
            {
                LobbyId = Guid.Parse(txtLobbyId.Text)
            }));

            DrawniteCore.Networking.Data.Message returnMessage = null;
            ManualResetEvent receivedSignal = new ManualResetEvent(false);
            (Application.Current as App).ClientWrapper.OnReceived += async (client, message) =>
            {
                returnMessage = message;
                receivedSignal.Set();
            };
            receivedSignal.WaitOne();

            int lobbyPort = returnMessage.Data.LobbyInfo.LobbyPort;
            bool result = (Application.Current as App).ClientWrapper.Forward(new System.Net.IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), lobbyPort));
            if (result)
            {
                (Application.Current as App).ClientWrapper.NetworkConnection.Write(new Message("player/join", new
                {
                    Username = username,
                    GUID = returnMessage.Data.PlayerGuid
                }));
            }

            Guid lobbyId = returnMessage.Data.LobbyInfo.LobbyId;
            Guid playerId = returnMessage.Data.PlayerGuid;
            parentFrameView.Navigate(new Page1(lobbyId, playerId));
        }

        private async void OnHostBttn(object sender, RoutedEventArgs e)
        {
            LoginDialogData data = await (Application.Current.MainWindow as MainWindow).ShowLoginAsync("Host", "Enter username");
            (Application.Current as App).ClientWrapper.NetworkConnection.Write(new DrawniteCore.Networking.Data.Message("host", new 
            { 
                Username = data.Username,
                Password = data.Password
            }));

            DrawniteCore.Networking.Data.Message returnMessage = null;
            ManualResetEvent receivedSignal = new ManualResetEvent(false);
            (Application.Current as App).ClientWrapper.OnReceived += async (client, message) => 
            {
                returnMessage = message;
                receivedSignal.Set();
            };
            receivedSignal.WaitOne();

            int lobbyPort = returnMessage.Data.LobbyInfo.LobbyPort;
            bool result = (Application.Current as App).ClientWrapper.Forward(new System.Net.IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), lobbyPort));
            if (result)
            {
                (Application.Current as App).ClientWrapper.NetworkConnection.Write(new Message("player/join", new
                {
                    Username = data.Username,
                    GUID = returnMessage.Data.PlayerGuid
                }));
            }

            Guid lobbyId = returnMessage.Data.LobbyInfo.LobbyId;
            Guid playerId = returnMessage.Data.PlayerGuid;
            parentFrameView.Navigate(new Page1(lobbyId, playerId));
        }
    }
}
