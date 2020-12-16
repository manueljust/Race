using Race.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Race
{
    public class NetworkConnector : PropertyChangedAware
    {
        private TcpClient _client;

        public string IPAddress { get; }

        private bool _lockInTrack = false;
        public bool LockInTrack
        {
            get { return _lockInTrack; }
            set { SetProperty(ref _lockInTrack, value); }
        }

        private bool _isReady = false;
        public bool IsReady
        {
            get { return _isReady; }
            set { SetProperty(ref _isReady, value); }
        }

        public NetworkConnector(TcpClient client)
        {
            _client = client;
            IPAddress = client.Client.RemoteEndPoint.AddressFamily.ToString();
        }

        public async Task<Car> GetRemoteCar()
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetRemoteCar waiting for info.");
            Car car = (Car)(await _client.GetStream().ReadMessageAsync(PayloadType.Car)).Payload;
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetRemoteCar received car of {car.Driver}.");
            car.PlayerType = PlayerType.Online;
            car.NetworkConnector = this;
            return car;
        }

        public async Task<MoveParameter> GetMoveParameter()
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetMoveParameter waiting for info.");
            MoveParameter move = (MoveParameter)(await _client.GetStream().ReadMessageAsync(PayloadType.MoveParameter)).Payload;
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetMoveParameter received move ({move.Angle},{move.Power}).");
            return move;
        }

        public async Task<NewGameDialogResult> GetTrackInfo()
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetTrackInfo waiting for info.");
            NewGameDialogResult result = (NewGameDialogResult)(await _client.GetStream().ReadMessageAsync(PayloadType.NewGameDialogResult)).Payload;
            result.Cars = new ObservableCollection<Car>();
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetTrackInfo received track {result.TrackFileName}.");
            return result;
        }

        public async Task ConfirmStart(Car car)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.ConfirmStart sent car of {car.Driver}.");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.Car,
                Payload = car,
            });
        }

        public async Task SendTrackInfo(NewGameDialogResult result)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.SendTrackInfo sent track {result.TrackFileName}.");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.NewGameDialogResult,
                Payload = result,
            });
        }

        public async Task ConfirmMove(MoveParameter move)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.ConfirmMove sent move ({move.Angle}, {move.Power}).");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.MoveParameter,
                Payload = move,
            });
        }

        public async Task ConfirmLockIn(bool lockedIn)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.ConfirmLockIn sent 'locked {(lockedIn ? "in" : "out")}'.");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.LockInTrack,
                Payload = lockedIn,
            });
        }
    }
}
