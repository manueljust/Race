using Race.Util;
using System;
using System.Collections.Concurrent;
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
        private Task _requestHandler;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private ConcurrentQueue<string> _inBuffer = new ConcurrentQueue<string>();


        public NetworkConnector(TcpClient client)
        {
            _client = client;
            _writer = new StreamWriter(client.GetStream());
            _reader = new StreamReader(client.GetStream());

            _requestHandler = new Task(async () =>
            {
                while(!_cts.IsCancellationRequested)
                {
                    // whenany returns the task that completed first
                    // await that task as well (hence double await)
                    string s = await await Task.WhenAny(_reader.ReadLineAsync(), GetDelayedNullString(_cts.Token, TimeSpan.FromMilliseconds(100)));

                    if (null != s)
                    {
                        _inBuffer.Enqueue(s);
                    }
                }
            },
            _cts.Token,
            TaskCreationOptions.LongRunning);
        }

        private static async Task<string> GetDelayedNullString(CancellationToken token, TimeSpan delay)
        {
            await Task.Run(() => token.WaitHandle.WaitOne(delay));
            return null;
        }

        public void Close()
        {
            _writer.Close();
            _reader.Close();
            _client.Close();
            _cts.Cancel();
            _requestHandler.Wait();
        }

        public async Task<Car> GetRemoteCar()
        {
            return Car.FromString(await GetResponse("getCar"));
        }

        public async Task<MoveParameter> GetMoveParameter()
        {
            return MoveParameter.FromString(await GetResponse("getMove"));
        }

        public void ConfirmStart(Car car)
        {
            _writer.WriteLine(car.GetStringRepresentation());
            _writer.Flush();
        }

        public void ConfirmMove(MoveParameter move)
        {
            _writer.WriteLine(move.GetStringRepresentation());
            _writer.Flush();
        }

        private async Task<string> GetResponse(string query, TimeSpan timeout = default)
        {
            _writer.WriteLine(query);
            _writer.Flush();
            return await _inBuffer.DequeueAsync(_cts.Token, timeout);
        }
    }
}
