using DrawniteCore.Networking.Data;
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
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;

namespace DrawniteClient.Views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class GamePage : NetworkPage
    {
        private Guid lobbyId;
        private Guid playerId;
        private bool isDrawer;
        private string selectedWord;

        public GamePage(Guid lobbyId, Guid playerId)
        {
            InitializeComponent();
            this.lobbyId = lobbyId;
            this.playerId = playerId;
            this.isDrawer = false;

            this.NetworkConnection.OnReceived += OnReceived;
        }

        private async void OnReceived(DrawniteCore.Networking.IConnection sender, dynamic args)
        {
            Message msg = (Message)args;
            switch (msg.Command)
            {
                case "game/selected":
                    await Dispatcher.Invoke(async () =>
                    {
                        isDrawer = true;
                        selectedWord = msg.Data.Word;

                        await this.ParentWindow.ShowMessageAsync("Your up!", $"You need to draw: {selectedWord}", MessageDialogStyle.Affirmative);

                        NetworkConnection.Write(new Message("game/selected", new { }));
                        drawingCanvas.IsEnabled = true;
                    });
                break;

                case "game/awaiting":
                    isDrawer = false;
                    Dispatcher.Invoke(() =>
                    {
                        drawingCanvas.IsEnabled = false;
                    });

                    Dispatcher.Invoke(() =>
                    {
                        this.ParentWindow.ShowOverlay();
                    });
                break;

                case "game/starting":
                    Dispatcher.Invoke(() =>
                    {
                        this.ParentWindow.HideOverlay();
                    });
                break;

                case "game/playing":
                    {
                        Dispatcher.Invoke(() =>
                        {
                            int timeLeft = msg.Data.TimeLeft;
                            List<Player> playerList = (msg.Data.PlayerList as JArray).ToObject<List<Player>>();
                            this.ParentWindow.Title = $"Time Left: {timeLeft}";
                        });
                    }
                break;

                case "game/chat":
                    {
                        Dispatcher.Invoke(() =>
                        {
                            guess_textblock.Text += msg.Data.Message + Environment.NewLine;
                        });
                    }
                break;

                case "game/end":
                    Dispatcher.Invoke(() =>
                    {
                        this.NavigationService.GoBack();
                    });
                break;
            }
        }
    }
}
