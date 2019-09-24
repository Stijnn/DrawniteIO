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
        private TcpClientWrapper clientWrapper;
        public TcpClientWrapper ClientWrapper => clientWrapper;

        public App()
        {
            this.clientWrapper = new TcpClientWrapper();
            clientWrapper.OnReceived += OnReceived;
            clientWrapper.Connect(new IPEndPoint(IPAddress.Parse(Constants.SERVER_IP), Constants.AUTH_PORT));
        }

        private void OnReceived(IConnection client, dynamic args)
        {
            Console.WriteLine(Encoding.ASCII.GetString(args));
        }
    }
}
