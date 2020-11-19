using Microsoft.Win32;
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
        public class Player : PropertyChangedAware
        {
            private string _name = "Name";
            public string Name
            {
                get { return _name; }
                set { SetProperty(ref _name, value); }
            }

            private Color _color = Colors.Red;
            public Color Color
            {
                get { return _color; }
                set { SetProperty(ref _color, value); }
            }

        }

        public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>()
        {
            new Player() { Name = "Manu", Color = Colors.Teal }
        };

        public string TrackFileName { get; set; }

        public RaceDirection RaceDirection { get; set; }

        public NewGameDialog()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            TrackFileName = TextBoxTrackFileName.Text;
            RaceDirection = true == RadioButtonCW.IsChecked ? RaceDirection.Clockwise : RaceDirection.Counterclockwise;

            base.OnClosing(e);
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                TextBoxTrackFileName.Text = dlg.FileName;
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
