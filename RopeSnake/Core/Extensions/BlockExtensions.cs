using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public static class BlockExtensions
    {
        public static sbyte ReadSByte(this Block block, int offset)
            => (sbyte)block[offset];

        public static void WriteSByte(this Block block, sbyte value, int offset)
            => block[offset] = (byte)value;

        public static short ReadShort(this Block block, int offset, Endianness endian = Endianness.Little)
        {
            switch (endian)
            {
                case Endianness.Little:
                    return (short)(block[offset] | (block[offset + 1] << 8));

                case Endianness.Big:
                    return (short)(block[offset + 1] | (block[offset] << 8));

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static void WriteShort(this Block block, short value, int offset, Endianness endian = Endianness.Little)
        {
            switch (endian)
            {
                case Endianness.Little:
                    block[offset++] = (byte)(value & 0xFF);
                    block[offset++] = (byte)((value >> 8) & 0xFF);
                    return;

                case Endianness.Big:
                    block[offset++] = (byte)((value >> 8) & 0xFF);
                    block[offset++] = (byte)(value & 0xFF);
                    return;

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static ushort ReadUShort(this Block block, int offset, Endianness endian = Endianness.Little)
            => (ushort)block.ReadShort(offset, endian);

        public static void WriteUShort(this Block block, ushort value, int offset, Endianness endian = Endianness.Little)
            => block.WriteShort((short)value, offset, endian);

        public static int ReadInt(this Block block, int offset, Endianness endian = Endianness.Little)
        {
            switch (endian)
            {
                case Endianness.Little:
                    return block.ReadUShort(offset) | (block.ReadUShort(offset + 2) << 16);

                case Endianness.Big:
                    return block.ReadUShort(offset + 2, Endianness.Big) | (block.ReadUShort(offset, Endianness.Big) << 16);

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static void WriteInt(this Block block, int value, int offset, Endianness endian = Endianness.Little)
        {
            switch (endian)
            {
                case Endianness.Little:
                    block.WriteShort((short)(value & 0xFFFF), offset);
                    block.WriteShort((short)((value >> 16) & 0xFFFF), offset + 2);
                    return;

                case Endianness.Big:
                    block.WriteShort((short)(value & 0xFFFF), offset + 2, Endianness.Big);
                    block.WriteShort((short)((value >> 16) & 0xFFFF), offset, Endianness.Big);
                    return;

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static uint ReadUInt(this Block block, int offset, Endianness endian = Endianness.Little)
            => (uint)block.ReadInt(offset, endian);

        public static void WriteUInt(this Block block, uint value, int offset, Endianness endian = Endianness.Little)
            => block.WriteInt((int)value, offset, endian);

        public static string ReadString(this Block block, int offset)
            => block.ToStream(offset).ReadString();

        public static void WriteString(this Block block, string value, int offset)
            => block.ToStream(offset).WriteString(value);

        public static string ReadString(this Block block, int offset, int byteCount)
            => block.ToStream(offset).ReadString(byteCount);

        public static void WriteString(this Block block, string value, int offset, int byteCount)
            => block.ToStream(offset).WriteString(value, byteCount);
    }

    public enum Endianness
    {
        Little,
        Big
    }
}
