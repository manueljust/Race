using Race.Util;
using System;
using System.Collections.Generic;

namespace Race
{
    public class NetworkMessage
    {
        public static byte StartByte { get; } = 0xC9;
        public static byte StopByte { get; } = 0x63;

        public PayloadType PayloadType { get; set; } = PayloadType.Unset;
        public object Payload { get; set; }

        public byte[] GetBytes()
        {
            byte[] payloadBytes = Serializer.GetBytes(Payload, PayloadType);

            int length = 1 + 4 + 1 + payloadBytes.Length + 1;
            List<byte> buffer = new List<byte>();
            buffer.Add(StartByte);
            buffer.AddRange(BitConverter.GetBytes(length));
            buffer.Add((byte)PayloadType);
            buffer.AddRange(payloadBytes);
            buffer.Add(StopByte);
            return buffer.ToArray();
        }

        internal static NetworkMessage FromBytes(byte[] messageBuffer)
        {
            if(messageBuffer[0] != StartByte)
            {
                throw new ArgumentException($"Message.FromBytes start byte {messageBuffer[0]} did not match expected start byte {StartByte}.");
            }
            if (messageBuffer[messageBuffer.Length - 1] != StopByte)
            {
                throw new ArgumentException($"Message.FromBytes stop byte {messageBuffer[messageBuffer.Length - 1]} did not match expected stop byte {StopByte}.");
            }
            int length = BitConverter.ToInt32(messageBuffer, 1);
            if (messageBuffer.Length != length)
            {
                throw new ArgumentException($"Message.FromBytes length {messageBuffer.Length} did not match expected length {length}.");
            }
            PayloadType type = (PayloadType)messageBuffer[5];
            NetworkMessage message = new NetworkMessage()
            {
                PayloadType = type,
                Payload = Serializer.ToObject(type, messageBuffer, 6, length - 7),
            };
            return message;
        }
    }
}
