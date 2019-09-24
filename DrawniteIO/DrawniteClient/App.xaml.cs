using DrawniteCore.Networking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawniteClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            NetworkService network = new NetworkService();
            network.OnDataReceived += Network_OnDataReceived;
            network.Connect(new System.Net.IPEndPoint(IPAddress.Parse("145.49.23.205"), 20000));
            network.SendData(Encoding.ASCII.GetBytes("faka man bro"));
        }

        private void Network_OnDataReceived(NetworkService service, object args)
        {
            Debug.WriteLine(Encoding.ASCII.GetString((byte[]) args));

        }
    }
}
