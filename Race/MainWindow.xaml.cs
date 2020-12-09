using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Race
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Game Game { get; } = new Game();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void thisMW_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if(PlayerType.Human == Game.ActiveCar.PlayerType)
                {
                    await Game.Move();
                }
            }
        }

        private async void Move_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerType.Human == Game.ActiveCar.PlayerType)
            {
                await Game.Move();
            }
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            NewGameDialog dlg = new NewGameDialog();
            dlg.ShowDialog();

            Game.NewGame(dlg.Result.TrackFileName, dlg.Result.Cars, dlg.Result.RaceDirection, "local");

            e.Handled = true;
        }

        private void JoinOnline_Click(object sender, RoutedEventArgs e)
        {
            JoinOnlineGameDialog dlg = new JoinOnlineGameDialog();
            dlg.ShowDialog();

            Game.NewGame(dlg.Result.TrackFileName, dlg.Result.Cars, dlg.Result.RaceDirection, "guest");
        }

        private void HostOnline_Click(object sender, RoutedEventArgs e)
        {
            HostOnlineGameDialog dlg = new HostOnlineGameDialog();
            dlg.ShowDialog();

            Game.NewGame(dlg.Result.TrackFileName, dlg.Result.Cars, dlg.Result.RaceDirection, "host");
        }
    }
}
