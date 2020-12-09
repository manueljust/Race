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
using System.Net.Sockets;
using Microsoft.Win32;

namespace Race
{
    /// <summary>
    /// Interaction logic for HostOnlineGameDialog.xaml
    /// </summary>
    public partial class HostOnlineGameDialog : Window
    {
        public NewGameDialogResult Result { get; } = new NewGameDialogResult();

        public HostOnlineGameDialog()
        {
            InitializeComponent();
            Result.Cars.Clear();
            Result.Cars.Add(new Car() { Driver = "Host", Color = Colors.Blue });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(Car car in Result.Cars)
            {
                if(PlayerType.Online == car.PlayerType)
                {
                    car.NetworkConnector.ConfirmStart(Result.Cars.First());
                }
            }

            DialogResult = true;
            Close();
        }

        private async void ButtonHost_Click(object sender, RoutedEventArgs e)
        {
            trackSelector.IsEnabled = false;
            directionSelector.IsEnabled = false;
            infoBox.Text = $"Waiting for opponent.";
            string local = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => AddressFamily.InterNetwork == ip.AddressFamily).Select(ip => ip.ToString()).FirstOrDefault();
            if(null != local && IPAddress.TryParse(local, out IPAddress address))
            {
                TcpListener listener = new TcpListener(address, 5001);
                infoBox.Text += $" connected to {address} on 5001.";
                listener.Start();

                // get driver, color and powershape from remote
                NetworkConnector c = new NetworkConnector(await listener.AcceptTcpClientAsync());
                infoBox.Text += $" accepted connection from {c}. waiting for opponent to chose car.";
                c.SendTrackInfo(Result);

                Car car = await c.GetRemoteCar();
                car.PlayerType = PlayerType.Online;
                car.NetworkConnector = c;
                Result.Cars.Add(car);
                infoBox.Text += $" {car.Driver} is ready to start.";

                startButton.IsEnabled = true;
            }
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                Result.TrackFileName = dlg.FileName;
            }
        }
    }
}
