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

namespace Race
{
    /// <summary>
    /// Interaction logic for NewOnlineGameDialog.xaml
    /// </summary>
    public partial class JoinOnlineGameDialog : Window
    {
        public NewGameDialogResult Result { get; set; } = new NewGameDialogResult();
        private NetworkConnector _networkConnector = null;

        public JoinOnlineGameDialog()
        {
            InitializeComponent();
            startButton.IsEnabled = false;
        }

        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            _networkConnector.ConfirmStart(Result.Cars.First());

            infoBox.Text += " waiting for hosst to start";
            Result.Cars.Insert(0, await _networkConnector.GetRemoteCar());

            DialogResult = true;
            Close();
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (IPAddress.TryParse(ipBox.Text, out IPAddress address))
            {
                infoBox.Text += $" ip {address} parsed successfully.";

                //string local = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => AddressFamily.InterNetwork == ip.AddressFamily).Select(ip => ip.ToString()).FirstOrDefault();
                string local = "127.0.0.1";
                if (null != local && IPAddress.TryParse(local, out IPAddress localAddress))
                {
                    TcpClient client = new TcpClient(new IPEndPoint(localAddress, 5002));
                    client.Connect(new IPEndPoint(address, 5001));
                    if (client.Connected)
                    {
                        ipStuff.IsEnabled = false;
                        _networkConnector = new NetworkConnector(client);
                        infoBox.Text += $" connected to {address}, waiting for track info.";
                        Result = await _networkConnector.GetTrackInfo();
                        Car car = new Car()
                        {
                            Driver = "Guest",
                        };
                        Result.Cars.Add(car);
                        startButton.IsEnabled = true;
                        infoBox.Text += $" ok. let's race on {Result.TrackFileName} in {Result.RaceDirection} direction. choose your car and hit start";
                    }
                    else
                    {
                        infoBox.Text += " could not connect.";
                    }
                }
                else
                {
                    infoBox.Text += " networking error.";
                }
            }
        }
    }
}
