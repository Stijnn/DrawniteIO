﻿using System;
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

namespace DrawniteClient.Views.Controls
{
    /// <summary>
    /// Interaction logic for ConnectedPlayer.xaml
    /// </summary>
    public partial class ConnectedPlayer : UserControl
    {
        public string PlayerName => playerName;
        private string playerName;

        public ConnectedPlayer(string playerName, bool isLeader)
        {
            this.playerName = playerName;
            InitializeComponent();
            icoPlayer.Kind = isLeader ? MahApps.Metro.IconPacks.PackIconJamIconsKind.CrownF : MahApps.Metro.IconPacks.PackIconJamIconsKind.UserPlus;
            txtUsername.Text = playerName;
        }
    }
}
