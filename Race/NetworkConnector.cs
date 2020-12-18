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

        public AsyncEventHandler<Car> Ready;
        public AsyncEventHandler<bool> LockedIn;
        public AsyncEventHandler<NewGameDialogResult> TrackInfoChanged;
        public AsyncEventHandler<MoveParameter> Moved;

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
            IPAddress = ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).ToString();
            Task.Run(ReadLoop);
        }

        private async Task ReadLoop()
        {
            while(_client.GetStream().CanRead)
            {
                NetworkMessage message = await _client.GetStream().ReadMessageAsync();
                if(null == message)
                {
                    System.Diagnostics.Debug.WriteLine($"NetworkConnector.ReadLoop {IPAddress} discarding null message.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"NetworkConnector.ReadLoop {IPAddress} received {message.PayloadType} message.");
                    switch (message.PayloadType)
                    {
                        case PayloadType.Car:
                            IsReady = true;
                            if (null != Ready)
                            {
                                await Ready.InvokeAllAsync(this, (Car)message.Payload);
                            }
                            break;
                        case PayloadType.LockInTrack:
                            LockInTrack = (bool)message.Payload;
                            if (null != LockedIn)
                            {
                                await LockedIn.InvokeAllAsync(this, (bool)message.Payload);
                            }
                            break;
                        case PayloadType.NewGameDialogResult:
                            if (null != TrackInfoChanged)
                            {
                                await TrackInfoChanged.InvokeAllAsync(this, (NewGameDialogResult)message.Payload);
                            }
                            break;
                        case PayloadType.MoveParameter:
                            if (null != Moved)
                            {
                                await Moved.InvokeAllAsync(this, (MoveParameter)message.Payload);
                            }
                            break;
                    }
                }
            }
        }

        public async Task<Car> GetRemoteCar()
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetRemoteCar {IPAddress} waiting for info.");
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            Car car = null;
            Ready += async (o, e) => { car = e; semaphore.Release(); };
            await semaphore.WaitAsync();

            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetRemoteCar {IPAddress} received car of {car.Driver}.");
            car.PlayerType = PlayerType.Online;
            car.NetworkConnector = this;
            return car;
        }

        public async Task<MoveParameter> GetMoveParameter()
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetMoveParameter {IPAddress} waiting for info.");
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            MoveParameter move = null;
            Moved += async (o, e) => { move = e; semaphore.Release(); };
            await semaphore.WaitAsync();

            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetMoveParameter {IPAddress} received move ({move.Angle},{move.Power}).");
            return move;
        }

        public async Task<NewGameDialogResult> GetTrackInfo()
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetTrackInfo {IPAddress} waiting for info.");
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            NewGameDialogResult result = null;
            TrackInfoChanged += async (o, e) => { result = e; semaphore.Release(); };
            await semaphore.WaitAsync();

            result.Cars = new ObservableCollection<Car>();
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.GetTrackInfo {IPAddress} received track {result.TrackFileName}.");
            return result;
        }

        public async Task ConfirmStart(Car car)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.ConfirmStart {IPAddress} sent car of {car.Driver}.");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.Car,
                Payload = car,
            });
        }

        public async Task SendTrackInfo(NewGameDialogResult result)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.SendTrackInfo {IPAddress} sent track {result.TrackFileName}, {result.RaceDirection}.");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.NewGameDialogResult,
                Payload = result,
            });
        }

        public async Task ConfirmMove(MoveParameter move)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.ConfirmMove {IPAddress} sent move ({move.Angle}, {move.Power}).");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.MoveParameter,
                Payload = move,
            });
        }

        public async Task ConfirmLockIn(bool lockedIn)
        {
            System.Diagnostics.Debug.WriteLine($"NetworkConnector.ConfirmLockIn {IPAddress} sent lockIn:{lockedIn}.");
            await _client.GetStream().Send(new NetworkMessage()
            {
                PayloadType = PayloadType.LockInTrack,
                Payload = lockedIn,
            });
        }
    }
}
