using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
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
using DrawniteCore.Networking.Data;
using System.Windows.Interop;
using Image = System.Windows.Controls.Image;

namespace DrawniteClient.Views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class GamePage : NetworkPage
    {
        public GamePage(Guid lobbyId, Guid playerId)
        {
            InitializeComponent();
            this.NetworkConnection.OnReceived += NetworkConnection_OnReceived;
            for (int i = 0; i < 10; i++)
            {
                WriteLn("big nigga");
            }
            

        }
        private void NetworkConnection_OnReceived(DrawniteCore.Networking.IConnection sender, dynamic args)
        {
            Message message = (Message)args;
            switch (message.Command)
            {
                case "canvas/update":
                    {
                        
                        Dispatcher.Invoke(() => {
                            var photo = new BitmapImage();
                            using (var ms = new MemoryStream())
                            {
                                byte[] a = Convert.FromBase64String(message.Data);
                                ms.Write(a, 0, a.Length);
                                ms.Position = 0;

                                photo.BeginInit();
                                photo.CacheOption = BitmapCacheOption.OnLoad;
                                photo.StreamSource = ms;
                                photo.EndInit();
                            }
                            Image img = new Image();
                            img.Source = photo;

                            ImageBrush brush = new ImageBrush();
                            brush.ImageSource = img.Source;

                            MainDrawingCanvas.Background = brush;
                        });
                       
                    }
                    break;
            }
        }


        private void WriteLn(string text)
        {
            guess_textblock.Text += text + Environment.NewLine; // tbLog is a TextBlock
        }

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            NetworkConnection.Write(new Message("canvas/update", Convert.ToBase64String(SignatureToBitmapBytes())));

        }

        private byte[] SignatureToBitmapBytes()
        {
            //get the dimensions of the ink control
            int margin = (int)this.MainDrawingCanvas.Margin.Left;
            int width = (int)this.MainDrawingCanvas.ActualWidth - margin;
            int height = (int)this.MainDrawingCanvas.ActualHeight - margin;
            //render ink to bitmap
            RenderTargetBitmap rtb =
            new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(MainDrawingCanvas);
            //save the ink to a memory stream
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            byte[] bitmapBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                //get the bitmap bytes from the memory stream
                ms.Position = 0;
                bitmapBytes = ms.ToArray();
            }
            return bitmapBytes;
        }


    }
}
