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
using System.IO;
using System.Timers;

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

        //private void MainDrawingCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        //{
        //    if (isDrawer)
        //        NetworkConnection.Write(new Message("canvas/update", new { Image = Convert.ToBase64String(SignatureToBitmapBytesAsync()) }));
        //}

        //private byte[] SignatureToBitmapBytesAsync()
        //{
        //    byte[] returned = null;
        //    Dispatcher.Invoke(() =>
        //    {
        //        int margin = (int)this.MainDrawingCanvas.Margin.Left;
        //        int width = (int)this.MainDrawingCanvas.ActualWidth - margin;
        //        int height = (int)this.MainDrawingCanvas.ActualHeight - margin;
        //        RenderTargetBitmap rtb =
        //        new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
        //        rtb.Render(MainDrawingCanvas);
        //        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(rtb));
        //        byte[] bitmapBytes;
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            encoder.Save(ms);
        //            ms.Position = 0;
        //            bitmapBytes = ms.ToArray();
        //        }
        //        returned = bitmapBytes;
        //    });

        //    return returned;
        //}


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
                        parentCanvas.IsEnabled = true;
                        //Panel.SetZIndex(imageGrid, -1);
                    });
                break;

                case "game/awaiting":
                    isDrawer = false;
                    Dispatcher.Invoke(() =>
                    {
                        parentCanvas.IsEnabled = false;
                    });

                    Dispatcher.Invoke(() =>
                    {
                        this.ParentWindow.ShowOverlay();
                        //Panel.SetZIndex(imageGrid, 1);

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
                            try
                            {
                                this.ParentWindow.HideOverlay();
                            }
                            catch (Exception e) {}

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

                case "canvas/update":
                    {
                        Dispatcher.Invoke(() => {
                            ImageBrush brush = new ImageBrush();
                            byte[] a = msg.Data.Image;
                            brush.ImageSource = ToImage(a);
                            parentCanvas.Background = brush;
                        });
                    }
                break;

            }
        }

        private Point currentPoint = new Point();

        private void parentCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDrawer)
                return;

            Dispatcher.Invoke(() =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    currentPoint = e.GetPosition(this);
                }
            });
        }

        private void parentCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawer)
                return;

            Dispatcher.Invoke(() =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Line line = new Line();

                    line.Stroke = SystemColors.WindowFrameBrush;
                    line.X1 = currentPoint.X;
                    line.Y1 = currentPoint.Y;
                    line.X2 = e.GetPosition(this).X;
                    line.Y2 = e.GetPosition(this).Y;

                    currentPoint = e.GetPosition(this);

                    parentCanvas.Children.Add(line);

                    
                    NetworkConnection.Write(new Message("canvas/update", new
                    {
                        Image = ExportToPng(parentCanvas)
                    }));
                }
            });
        }

        public byte[] ExportToPng(Canvas surface)
        {
            Transform transform = surface.LayoutTransform;
            surface.LayoutTransform = null;

            Size size = new Size(surface.ActualWidth, surface.ActualHeight);

            surface.Measure(size);
            surface.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            byte[] array = null;
            using (MemoryStream outStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(outStream);
                array = outStream.ToArray();
            }

            surface.LayoutTransform = transform;
            surface.Measure(size);
            Point relativePoint = surface.TransformToAncestor(gridCanvas).Transform(new Point(0, 0));
            surface.Arrange(new Rect(relativePoint, size));
            return array;
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private void TextBox_TouchEnter(object sender, TouchEventArgs e)
        {

        }

        private void txtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!isDrawer)
                {
                    NetworkConnection.Write(new Message("game/chat", new
                    {
                        Message = txtChat.Text,
                        IsDrawer = isDrawer
                    }));
                }

                txtChat.Text = "";
            }
        }
    }
}
