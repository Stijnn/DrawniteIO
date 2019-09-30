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
    using MahApps.Metro.Controls.Dialogs;

    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private async void OnConnectBttn(object sender, RoutedEventArgs e)
        {
            await (Application.Current.MainWindow as MainWindow).ShowMessageAsync("Connect", "You clicked connect");
            (Application.Current as App).ClientWrapper.NetworkConnection.Write(new DrawniteCore.Networking.Data.Message("join", null));

        }

        private async void OnHostBttn(object sender, RoutedEventArgs e)
        {
            await (Application.Current.MainWindow as MainWindow).ShowMessageAsync("Connect", "You clicked host");
            (Application.Current as App).ClientWrapper.NetworkConnection.Write(new DrawniteCore.Networking.Data.Message("host", null));
        }
    }
}
