using DrawniteClient.Views.Controls;
using DrawniteCore.Networking.Data;
using Newtonsoft.Json.Linq;
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
    /// <summary>
    /// Interaction logic for LobbyPage.xaml
    /// </summary>
    public partial class LobbyPage : NetworkPage
    {
        public LobbyPage(Guid lobbyId, Guid playerId)
        {
            InitializeComponent();
            this.NetworkConnection.OnReceived += OnPacketReceived;
            txtLobbyId.Text = lobbyId.ToString();
            this.NetworkConnection.Write(new Message("lobby/playerlist", null));
        }

        private void OnPacketReceived(DrawniteCore.Networking.IConnection connection, dynamic args)
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
                    Player player = ((JObject)args.Data.Player).ToObject<Player>();
                    Dispatcher.Invoke(() =>
                    {
                        Stack<int> removingIndices = new Stack<int>();
                        for (int i = 0; i < lvConnectedPlayers.Children.Count; i++)
                        {
                            ConnectedPlayer connectedPlayer = lvConnectedPlayers.Children[i] as ConnectedPlayer;
                            if (connectedPlayer.PlayerName == player.Username)
                                removingIndices.Push(i);
                        }

                        while (removingIndices.Count > 0)
                            lvConnectedPlayers.Children.RemoveAt(removingIndices.Pop());
                    });
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
                        }
                    });
                }
                break;
            }
        }
    }
}
