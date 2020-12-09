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
            infoBox.Text += " waiting for hosst to start";
            Car host = await _networkConnector.GetRemoteCar();
            host.PlayerType = PlayerType.Online;
            host.NetworkConnector = _networkConnector;
            Result.Cars.Add(host);

            DialogResult = true;
            Close();
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (IPAddress.TryParse(ipBox.Text, out IPAddress address))
            {
                infoBox.Text += $" ip {address} parsed successfully.";

                string local = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => AddressFamily.InterNetwork == ip.AddressFamily).Select(ip => ip.ToString()).FirstOrDefault();
                if (null != local && IPAddress.TryParse(local, out IPAddress localAddress))
                {
                    TcpClient client = new TcpClient(new IPEndPoint(localAddress, 5001));
                    client.Connect(new IPEndPoint(address, 5001));
                    if (client.Connected)
                    {
                        _networkConnector = new NetworkConnector(client);
                        Result = await _networkConnector.GetTrackInfo();
                        Car car = new Car()
                        {
                            Driver = "guest",
                        };
                        Result.Cars.Add(car);
                        ipStuff.IsEnabled = false;
                        startButton.IsEnabled = true;
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
