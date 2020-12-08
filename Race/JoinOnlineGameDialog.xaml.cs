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
using System.Windows.Shapes;
using System.Net;

namespace Race
{
    /// <summary>
    /// Interaction logic for NewOnlineGameDialog.xaml
    /// </summary>
    public partial class JoinOnlineGameDialog : Window
    {
        public NewGameDialogResult Result { get; } = new NewGameDialogResult();

        public JoinOnlineGameDialog()
        {
            InitializeComponent();
            Result.Cars.Clear();
            Result.Cars.Add(new Car() { Driver = "Guest", Color = Colors.Green });
            startButton.IsEnabled = false;
            ipBox.TextChanged += IpBox_TextChanged;
        }

        private void IpBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(IPAddress.TryParse(ipBox.Text, out IPAddress address))
            {
                infoBox.Text += $" ip {ipBox.Text} parsed successfully.";

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
