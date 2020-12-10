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
    public class NetworkConnector
    {
        private TcpClient _client;
        private StreamWriter _writer;
        private StreamReader _reader;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public NetworkConnector(TcpClient client)
        {
            _client = client;
            _writer = new StreamWriter(client.GetStream());
            _reader = new StreamReader(client.GetStream());
        }

        private async Task<string> ReadLineAsync()
        {
            try
            {
                // whenany returns the task that completed first
                // await that task as well (hence double await)
                return await await Task.WhenAny(_reader.ReadLineAsync(), Task.Run(() => { _cts.Token.WaitHandle.WaitOne(); return default(string); }));
            }
            catch
            {
                return default;
            }
        }

        public void Close()
        {
            _cts.Cancel();
            _writer.Close();
            _reader.Close();
            _client.Close();
        }

        public async Task<Car> GetRemoteCar()
        {
            Car car = Car.FromString(await GetResponse());
            car.NetworkConnector = this;
            return car;
        }

        public async Task<MoveParameter> GetMoveParameter()
        {
            return MoveParameter.FromString(await GetResponse());
        }

        public async Task<NewGameDialogResult> GetTrackInfo()
        {
            string resultString = await GetResponse();
            Dictionary<string, string> d = resultString.ToDictionary();
            return new NewGameDialogResult()
            {
                Cars = new ObservableCollection<Car>(),
                TrackFileName = d["track"],
                RaceDirection = (RaceDirection)Enum.Parse(typeof(RaceDirection), d["direction"]),
            };
        }

        public void ConfirmStart(Car car)
        {
            _writer.WriteLine(car.GetStringRepresentation());
            _writer.Flush();
        }

        internal void SendTrackInfo(NewGameDialogResult result)
        {
            _writer.WriteLine($"track:{result.TrackFileName},direction:{result.RaceDirection}");
            _writer.Flush();
        }

        public void ConfirmMove(MoveParameter move)
        {
            _writer.WriteLine(move.GetStringRepresentation());
            _writer.Flush();
        }

        private async Task<string> GetResponse()
        {
            return await ReadLineAsync();
        }
    }
}
