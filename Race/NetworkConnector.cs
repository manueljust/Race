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
        public class Message
        {
            public static byte StartByte { get; } = 0xC9;
            public static byte StopByte { get; } = 0x63;

            public enum PayloadType : byte
            {
                Car,
                TrackBytes,
                NewGameDialogResult,
                MoveParameter
            }

            public PayloadType Type { get; set; } = PayloadType.MoveParameter;
            public object Payload { get; set; }

            public byte[] GetBytes()
            {
                byte[] payloadBytes = Serializer.GetBytes(Payload, Type);

                int length = 1 + 2 + 4 + 1 + payloadBytes.Length + 1;
                List<byte> buffer = new List<byte>();
                buffer.Add(StartByte);
                buffer.Add(0x00); // placeholder crc high
                buffer.Add(0x00); // placeholder crc low
                buffer.AddRange(BitConverter.GetBytes(length));
                buffer.Add((byte)Type);
                buffer.AddRange(payloadBytes);
                buffer.Add(StopByte);
                byte[] crcSum = BitConverter.GetBytes(Crc16.ComputeChecksum(buffer));
                buffer[1] = crcSum[0];
                buffer[2] = crcSum[1];
                return buffer.ToArray();
            }

            internal static Message FromBytes(byte[] messageBuffer)
            {
                ushort crcSumExpected = BitConverter.ToUInt16(messageBuffer, 1);
                ushort crcSumActual = Crc16.ComputeChecksum(messageBuffer, 3);
                
                if(crcSumActual != crcSumExpected)
                {
                    throw new ArgumentException($"Message.FromBytes crc sum {crcSumActual} did not match expected crc sum {crcSumExpected}.");
                }
                if(messageBuffer[messageBuffer.Length - 1] != StopByte)
                {
                    throw new ArgumentException($"Message.FromBytes stop byte {messageBuffer[messageBuffer.Length - 1]} did not match expected stop byte {StopByte}.");
                }
                int length = BitConverter.ToInt32(messageBuffer, 3);
                if(messageBuffer.Length != length)
                {
                    throw new ArgumentException($"Message.FromBytes length {messageBuffer.Length} did not match expected length {length}.");
                }

                Message message = new Message()
                {
                    Type = (PayloadType)messageBuffer[7],
                    Payload = Serializer.ToObject((PayloadType)messageBuffer[7], messageBuffer, 8, length - 9),
                };
                return message;
            }
        }

        private class MessageReader
        {
            private ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();

            public MessageReader(Stream stream)
            {
                Task.Run(async () => await ReadNextMessage(stream));
            }

            private async Task ReadNextMessage(Stream stream)
            {
                byte[] headerBuffer = new byte[7];
                do
                {
                    if(!stream.CanRead || 1 != await stream.ReadAsync(headerBuffer, 0, 1))
                    {
                        System.Diagnostics.Debug.WriteLine($"MessageReader.ReadNextMessage could not read start byte.");
                        return;
                    }
                    if(Message.StartByte != headerBuffer[0])
                    {
                        System.Diagnostics.Debug.WriteLine($"MessageReader.ReadNextMessage skipped byte{headerBuffer[0]} before message start.");
                    }
                }
                while (Message.StartByte != headerBuffer[0]);

                if (!stream.CanRead || 6 != await stream.ReadAsync(headerBuffer, 1, 6))
                {
                    System.Diagnostics.Debug.WriteLine($"MessageReader.ReadNextMessage could not read length bytes.");
                    return;
                }
                int length = BitConverter.ToInt32(headerBuffer, 3);

                byte[] messageBuffer = new byte[length];
                if(!stream.CanRead || length - 5 != await stream.ReadAsync(messageBuffer, 5, length - 5))
                {
                    System.Diagnostics.Debug.WriteLine($"MessageReader.ReadNextMessage could not read message bytes.");
                    return;
                }

                Array.Copy(headerBuffer, messageBuffer, 5);

                try
                {
                    _messageQueue.Enqueue(Message.FromBytes(messageBuffer));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MessageReader.ReadNextMessage could not create and enqueue message. {ex}.");

                }
                finally
                {
                    await ReadNextMessage(stream);
                }
            }

            public async Task<Message> ReadMessageAsync(CancellationToken cancellationToken = default)
            {
                return await _messageQueue.DequeueAsync(cancellationToken);
            }
        }

        private TcpClient _client;
        private MessageReader _reader;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public NetworkConnector(TcpClient client)
        {
            _client = client;
            _reader = new MessageReader(_client.GetStream());
        }

        public void Close()
        {
            _cts.Cancel();
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
