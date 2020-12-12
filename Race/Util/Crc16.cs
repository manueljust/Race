using System.Collections.Generic;
/// <summary>
/// https://stackoverflow.com/a/22861111
/// </summary>
public static class Crc16
{
    private static ushort Polynomial { get; } = 0xA001;
    private static ushort[] Table { get; } = new ushort[256];

    public static ushort ComputeChecksum(IList<byte> bytes, int offset = 0, int length = default)
    {
        length = default == length ? bytes.Count - offset : length;

        ushort crc = 0;
        for (int i = offset; i < length; ++i)
        {
            byte index = (byte)(crc ^ bytes[i]);
            crc = (ushort)((crc >> 8) ^ Table[index]);
        }
        return crc;
    }

    static Crc16()
    {
        ushort value;
        ushort temp;
        for (ushort i = 0; i < Table.Length; ++i)
        {
            value = 0;
            temp = i;
            for (byte j = 0; j < 8; ++j)
            {
                if (((value ^ temp) & 0x0001) != 0)
                {
                    value = (ushort)((value >> 1) ^ Polynomial);
                }
                else
                {
                    value >>= 1;
                }
                temp >>= 1;
            }
            Table[i] = value;
        }
    }
}