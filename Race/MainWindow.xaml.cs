using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

            Loaded += async (o, e) => await Game.NewGame(@"Tracks\Track1.svg", Car.DefaultCars, 0 == new Random().Next() % 2 ? RaceDirection.Clockwise : RaceDirection.Counterclockwise, "local");
        }

        private async void thisMW_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await Game.ActiveCar.ConfirmMove();
            }
        }

        private async void Move_Click(object sender, RoutedEventArgs e)
        {
            await Game.ActiveCar.ConfirmMove();
        }

        private async void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            NewGameDialog dlg = new NewGameDialog();
            dlg.ShowDialog();
            e.Handled = true;

            await Game.NewGame(dlg.Result.TrackFileName, dlg.Result.Cars, dlg.Result.RaceDirection, "local");

        }

        private async void JoinOnline_Click(object sender, RoutedEventArgs e)
        {
            OnlineGameDialog dlg = new OnlineGameDialog();
            dlg.ShowDialog();

            await Game.NewGame(dlg.Result.TrackFileName, dlg.Result.Cars, dlg.Result.RaceDirection, "guest");
        }

        private async void CreateOnline_Click(object sender, RoutedEventArgs e)
        {
            OnlineGameDialog dlg = new OnlineGameDialog(true);
            dlg.ShowDialog();

            await Game.NewGame(dlg.Result.TrackFileName, dlg.Result.Cars, dlg.Result.RaceDirection, "host");
        }
    }
}
