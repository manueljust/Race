using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Race.Util
{
    public static class StreamExtensions
    {
        public static async Task<NetworkMessage> ReadMessageAsync(this Stream stream, PayloadType type)
        {
            do
            {
                NetworkMessage message = await stream.ReadMessageAsync();
                if(default == message)
                {
                    System.Diagnostics.Debug.WriteLine($"Stream.ReadMessageAsync({type}) discarding null message.");
                }
                else
                {
                    if (message.PayloadType == type)
                    {
                        return message;
                    }
                    System.Diagnostics.Debug.WriteLine($"Stream.ReadMessageAsync({type}) discarding message of type {message.PayloadType}.");
                }
            }
            while (stream.CanRead);
            return default;
        }

        public static async Task<NetworkMessage> ReadMessageAsync(this Stream stream)
        {
            byte[] headerBuffer = new byte[5];
            do
            {
                if (!stream.CanRead || 1 != await stream.ReadAsync(headerBuffer, 0, 1))
                {
                    System.Diagnostics.Debug.WriteLine($"Stream.ReadMessageAsync could not read start byte.");
                    return default;
                }
                if (NetworkMessage.StartByte != headerBuffer[0])
                {
                    System.Diagnostics.Debug.WriteLine($"Stream.ReadMessageAsync skipped byte {headerBuffer[0]:X} before message start.");
                }
            }
            while (NetworkMessage.StartByte != headerBuffer[0]);

            if (!stream.CanRead || 4 != await stream.ReadAsync(headerBuffer, 1, 4))
            {
                System.Diagnostics.Debug.WriteLine($"Stream.ReadMessageAsync could not read length bytes.");
                return default;
            }
            int length = BitConverter.ToInt32(headerBuffer, 1);

            byte[] messageBuffer = new byte[length];
            if (!stream.CanRead || length - 5 != await stream.ReadAsync(messageBuffer, 5, length - 5))
            {
                System.Diagnostics.Debug.WriteLine($"Stream.ReadMessageAsync could not read message bytes.");
                return default;
            }

            Array.Copy(headerBuffer, messageBuffer, 5);
            return NetworkMessage.FromBytes(messageBuffer);
        }

        public static async Task Send(this Stream stream, NetworkMessage message)
        {
            byte[] bytes = message.GetBytes();
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
