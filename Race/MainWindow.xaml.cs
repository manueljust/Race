﻿using System;
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

        private void thisMW_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Game.Move();
            }
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            Game.Move();
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            NewGameDialog dlg = new NewGameDialog();
            dlg.ShowDialog();

            Game.NewGame(dlg.Result.TrackFileName, dlg.Result.Cars, dlg.Result.RaceDirection);

            e.Handled = true;
        }
    }
}
