﻿using System; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Race.Util
{
    public static class Serializer
    {
        public static byte[] GetBytes(object obj, PayloadType type)
        {
            switch(type)
            {
                case PayloadType.Car:
                    return GetBytes((Car)obj);
                case PayloadType.MoveParameter:
                    return GetBytes((MoveParameter)obj);
                case PayloadType.NewGameDialogResult:
                    return GetBytes((NewGameDialogResult)obj);
                case PayloadType.TrackBytes:
                    return (byte[])obj;
                case PayloadType.LockInTrack:
                    return new byte[] { (bool)obj ? (byte)0x01 : (byte)0x00 };
                case PayloadType.CarOrder:
                    // used to transmit order of who plays next
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException($"Can not convert from type {type}.");
            }
        }

        internal static object ToObject(PayloadType type, byte[] messageBuffer, int offset, int length)
        {
            switch (type)
            {
                case PayloadType.Car:
                    return ToCar(messageBuffer, offset, length);
                case PayloadType.MoveParameter:
                    return ToMoveParameter(messageBuffer, offset, length);
                case PayloadType.NewGameDialogResult:
                    return ToNewGameDialogResult(messageBuffer, offset, length);
                case PayloadType.TrackBytes:
                    byte[] bytes = new byte[length];
                    Array.Copy(messageBuffer, offset, bytes, 0, length);
                    return bytes;
                case PayloadType.LockInTrack:
                    return 0x01 == messageBuffer[offset];
                case PayloadType.CarOrder:
                    // used to transmit order of who plays next
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException($"Can not convert to type {type}.");
            }
        }

        public static byte[] GetBytes(Car car)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(car.PowerShape.Acceleration));
            buffer.AddRange(BitConverter.GetBytes(car.PowerShape.Deceleration));
            buffer.AddRange(BitConverter.GetBytes(car.PowerShape.TurnRatio));
            buffer.AddRange(BitConverter.GetBytes(car.PowerShape.Edgyness));
            buffer.AddRange(BitConverter.GetBytes(car.PowerShape.Area));
            buffer.Add(car.Color.A);
            buffer.Add(car.Color.R);
            buffer.Add(car.Color.G);
            buffer.Add(car.Color.B);
            buffer.AddRange(Encoding.UTF8.GetBytes(car.Driver));
            return buffer.ToArray();
        }

        public static Car ToCar(byte[] bytes, int offset = 0, int length = default)
        {
            length = default == length ? bytes.Length - offset : length;
            Car car = new Car();
            car.PowerShape.Acceleration = BitConverter.ToDouble(bytes, 0 + offset);
            car.PowerShape.Deceleration = BitConverter.ToDouble(bytes, 8 + offset);
            car.PowerShape.TurnRatio = BitConverter.ToDouble(bytes, 16 + offset);
            car.PowerShape.Edgyness = BitConverter.ToDouble(bytes, 24 + offset);
            car.PowerShape.Area = BitConverter.ToDouble(bytes, 32 + offset);
            car.Color = Color.FromArgb(bytes[40 + offset], bytes[41 + offset], bytes[42 + offset], bytes[43 + offset]);
            car.Driver = Encoding.UTF8.GetString(bytes, 44 + offset, length - 44);
            return car;
        }

        public static byte[] GetBytes(MoveParameter move)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(move.Angle));
            buffer.AddRange(BitConverter.GetBytes(move.Power));
            return buffer.ToArray();
        }

        public static MoveParameter ToMoveParameter(byte[] bytes, int offset = 0, int length = default)
        {
            length = default == length ? bytes.Length - offset : length;
            return new MoveParameter(BitConverter.ToDouble(bytes, 0 + offset), BitConverter.ToDouble(bytes, 8 + offset));
        }

        public static byte[] GetBytes(NewGameDialogResult result)
        {
            List<byte> buffer = new List<byte>();
            buffer.Add((byte)result.RaceDirection);
            using (MD5 md5 = MD5.Create())
            using (FileStream fs = File.OpenRead(result.TrackFileName))
            {
                buffer.AddRange(md5.ComputeHash(fs));
            }
            buffer.AddRange(Encoding.UTF8.GetBytes(result.TrackFileName));
            return buffer.ToArray();
        }

        public static NewGameDialogResult ToNewGameDialogResult(byte[] bytes, int offset = 0, int length = default)
        {
            length = default == length ? bytes.Length - offset : length;
            byte[] md5 = new byte[16];
            Array.Copy(bytes, 1 + offset, md5, 0, 16);
            return new NewGameDialogResult()
            {
                RaceDirection = (RaceDirection)bytes[0 + offset],
                TrackFileName = Encoding.UTF8.GetString(bytes, 17 + offset, length - 17),
                TrackSum = md5,
            };
        }
    }
}
