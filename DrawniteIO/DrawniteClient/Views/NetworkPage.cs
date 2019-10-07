using DrawniteCore.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DrawniteClient.Views
{
    public class NetworkPage : Page
    {
        public TcpClientWrapper NetworkClient => (App.Current as App).ClientWrapper;
        public IConnection NetworkConnection => NetworkClient.NetworkConnection;
    }
}
