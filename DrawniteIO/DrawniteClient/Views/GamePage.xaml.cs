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
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class GamePage : NetworkPage
    {
        public GamePage(Guid lobbyId, Guid playerId)
        {
            InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                WriteLn("big nigga");
            }
            

        }

        private void WriteLn(string text)
        {
            guess_textblock.Text += text + Environment.NewLine; // tbLog is a TextBlock
        }
    }
}
