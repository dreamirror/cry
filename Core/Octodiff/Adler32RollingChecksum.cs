using System;

namespace Octodiff.Core
{
    public interface IRollingChecksum
    {
        string Name { get; }
        UInt32 Calculate(byte[] block, int offset, int count);
        UInt32 Rotate(UInt32 checksum, byte remove, byte add, int chunkSize);
    }

    public class Adler32RollingChecksum : IRollingChecksum
    {
        public string Name { get { return "Adler32"; } }

        public UInt32 Calculate(byte[] block, int offset, int count)
        {
            return Adler32RollingChecksum.CalculateChecksum(block, offset, count);
        }

        public UInt32 Rotate(UInt32 checksum, byte remove, byte add, int chunkSize)
        {
            return RotateChecksum(checksum, remove, add, chunkSize);
        }

        static public UInt32 CalculateChecksum(byte[] block, int offset = 0, int count = 0)
        {
            if (count == 0)
                count = block.Length;

            ushort a = 1;
            ushort b = 0;
            for (var i = offset; i < offset + count; i++)
            {
                var z = block[i];
                a = (ushort)(z + a);
                b = (ushort)(b + a);
            }
            return (UInt32) ((b << 16) | a);
        }

        static public UInt32 RotateChecksum(UInt32 checksum, byte remove, byte add, int chunkSize)
        {
            ushort b = (ushort)(checksum >> 16 & 0xffff);
            ushort a = (ushort)(checksum & 0xffff);

            a = (ushort)((a - remove + add));
            b = (ushort)((b - (chunkSize * remove) + a - 1));

            return (UInt32) ((b << 16) | a);
        }

        static public UInt32 Add(UInt32 checksum, byte add)
        {
            ushort b = (ushort)(checksum >> 16 & 0xffff);
            ushort a = (ushort)(checksum & 0xffff);

            a = (ushort)(add + a);
            b = (ushort)(b + a);

            return (UInt32)((b << 16) | a);
        }

        static public UInt32 Add(UInt32 checksum, byte[] block)
        {
            ushort b = (ushort)(checksum >> 16 & 0xffff);
            ushort a = (ushort)(checksum & 0xffff);

            for (var i = 0; i < block.Length; i++)
            {
                var z = block[i];
                a = (ushort)(z + a);
                b = (ushort)(b + a);
            }
            return (UInt32)((b << 16) | a);
        }

    }
}