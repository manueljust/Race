﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace Race
{
    /// <summary>
    /// Interaction logic for NewGameDialog.xaml
    /// </summary>
    public partial class NewGameDialog : Window
    {
        public class NewGameDialogResult : PropertyChangedAware
        {
            public ObservableCollection<Car> Cars { get; set; } = new ObservableCollection<Car>(Car.DefaultCars);

            private string _trackFileName = "Tracks/Track1.svg";
            public string TrackFileName
            {
                get { return _trackFileName; }
                set { SetProperty(ref _trackFileName, value); }
            }

            private RaceDirection _raceDirection = RaceDirection.Counterclockwise;
            public RaceDirection RaceDirection
            {
                get { return _raceDirection; }
                set { SetProperty(ref _raceDirection, value); }
            }
        }

        public NewGameDialogResult Result { get; } = new NewGameDialogResult();

        public NewGameDialog()
        {
            InitializeComponent();

            Color c = Colors.Red;
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                Result.TrackFileName = dlg.FileName;
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
