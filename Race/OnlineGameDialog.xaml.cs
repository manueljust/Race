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
using System.Collections.ObjectModel;

namespace Race
{
    /// <summary>
    /// Interaction logic for HostOnlineGameDialog.xaml
    /// </summary>
    public partial class OnlineGameDialog : Window
    {
        public static DependencyProperty LockedInProperty = DependencyProperty.Register(nameof(LockedIn), typeof(bool), typeof(OnlineGameDialog), new PropertyMetadata(false, OnLockedInChanged));
        public static DependencyProperty ReadyProperty = DependencyProperty.Register(nameof(Ready), typeof(bool), typeof(OnlineGameDialog), new PropertyMetadata(false));
        public static DependencyProperty CanChooseCarProperty = DependencyProperty.Register(nameof(CanChooseCar), typeof(bool), typeof(OnlineGameDialog), new PropertyMetadata(false));
        public static DependencyProperty CanChooseTrackProperty = DependencyProperty.Register(nameof(CanChooseTrack), typeof(bool), typeof(OnlineGameDialog), new PropertyMetadata(false));

        public bool LockedIn
        {
            get { return (bool)GetValue(LockedInProperty); }
            set { SetValue(LockedInProperty, value); }
        }

        private static void OnLockedInChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnlineGameDialog dlg = (OnlineGameDialog)d;
            dlg.CanChooseTrack = !(bool)e.NewValue;
            dlg.CanChooseCar = dlg.Connections.All(c => c.LockInTrack) && (bool)e.NewValue;
        }

        public bool Ready
        {
            get { return (bool)GetValue(ReadyProperty); }
            set { SetValue(ReadyProperty, value); }
        }

        public bool CanChooseCar
        {
            get { return (bool)GetValue(CanChooseCarProperty); }
            set { SetValue(CanChooseCarProperty, value); }
        }

        public bool CanChooseTrack
        {
            get { return (bool)GetValue(CanChooseTrackProperty); }
            set { SetValue(CanChooseTrackProperty, value); }
        }

        public NewGameDialogResult Result { get; } = new NewGameDialogResult();
        public ObservableCollection<NetworkConnector> Connections { get; } = new ObservableCollection<NetworkConnector>();

        public Car MyCar { get; } = new Car() { Driver = "Host", Color = Colors.Blue };


        public OnlineGameDialog(bool listenForConnections = false)
        {
            InitializeComponent();
            Result.Cars.Clear();
            Result.Cars.Add(MyCar);

            if(listenForConnections)
            {
                CanChooseTrack = true;

                //string local = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => AddressFamily.InterNetwork == ip.AddressFamily).Select(ip => ip.ToString()).FirstOrDefault();
                string local = "127.0.0.1";
                if (null != local && IPAddress.TryParse(local, out IPAddress address))
                {
                    TcpListener listener = new TcpListener(address, 5001);
                    infoBox.Text += $"\r\nListening on {address}:5001.";
                    listener.Start();

                    Loaded += async (o, e) => await GetNextConnectorAsync(listener);
                    //Task.Run(async () => await );
                }
            }
            else
            {
                // connect to host
                ipLabel.Visibility = Visibility.Visible;
                ipStuff.Visibility = Visibility.Visible;
            }
        }

        private async Task GetNextConnectorAsync(TcpListener listener)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            if(CanChooseCar)
            {
                infoBox.Text += $"\r\nAlready in choose car phase. Rejected connection from {client.Client.RemoteEndPoint}.";
                client.Close();
                listener.Stop();
                return;
            }

            NetworkConnector connector = new NetworkConnector(client);
            Connections.Add(connector);
            infoBox.Text += $"\r\naccepted connection from {client.Client.RemoteEndPoint}.";

            await connector.SendTrackInfo(Result);

            infoBox.Text += $"\r\ntrack info sent, listening for next connection.";
            await GetNextConnectorAsync(listener);
        }

        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            foreach(Car car in Result.Cars)
            {
                if(PlayerType.Online == car.PlayerType)
                {
                    await car.NetworkConnector.ConfirmStart(Result.Cars.First());
                }
            }

            DialogResult = true;
            Close();
        }

        private async void ButtonLockIn_Click(object sender, RoutedEventArgs e)
        {
            LockedIn = true;

            foreach(NetworkConnector connector in Connections)
            {
                await connector.ConfirmLockIn(true);
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

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (IPAddress.TryParse(ipBox.Text, out IPAddress address))
            {
                infoBox.Text += $"\r\nip {address} parsed successfully.";

                //string local = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => AddressFamily.InterNetwork == ip.AddressFamily).Select(ip => ip.ToString()).FirstOrDefault();
                string local = "127.0.0.1";
                if (null != local && IPAddress.TryParse(local, out IPAddress localAddress))
                {
                    TcpClient client = new TcpClient(new IPEndPoint(localAddress, 5002));
                    client.Connect(new IPEndPoint(address, 5001));
                    if (client.Connected)
                    {
                        ipLabel.IsEnabled = false;
                        ipStuff.IsEnabled = false;
                        CanChooseTrack = true;
                        NetworkConnector connector = new NetworkConnector(client);
                        Connections.Add(connector);
                        infoBox.Text += $"\r\nconnected to {address}:5001, waiting for track info.";
                        NewGameDialogResult result = await connector.GetTrackInfo();
                        Result.TrackFileName = result.TrackFileName;
                        Result.RaceDirection = result.RaceDirection;
                    }
                    else
                    {
                        infoBox.Text += "\r\ncould not connect.";
                    }
                }
                else
                {
                    infoBox.Text += "\r\nnetworking error.";
                }
            }
        }
    }
}
