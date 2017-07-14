using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public static class BlockExtensions
    { 
        internal const Endianness DefaultEndianness = Endianness.Little;

        public static Block ToBlock(this Stream stream)
            => stream.ToBlock((int)(stream.Length - stream.Position));

        public static Block ToBlock(this Stream stream, int length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, buffer.Length);
            return Block.Wrap(buffer);
        }

        public static byte ReadByte(this Block block, int offset)
            => block[offset];

        public static void WriteByte(this Block block, int offset, byte value)
            => block[offset] = value;

        public static sbyte ReadSByte(this Block block, int offset)
            => (sbyte)block[offset];

        public static void WriteSByte(this Block block, int offset, sbyte value)
            => block[offset] = (byte)value;

        public static short ReadShort(this Block block, int offset, Endianness endian = DefaultEndianness)
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

        public static void WriteShort(this Block block, int offset, short value, Endianness endian = DefaultEndianness)
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

        public static ushort ReadUShort(this Block block, int offset, Endianness endian = DefaultEndianness)
            => (ushort)block.ReadShort(offset, endian);

        public static void WriteUShort(this Block block, int offset, ushort value, Endianness endian = DefaultEndianness)
            => block.WriteShort(offset, (short)value, endian);

        public static int ReadInt(this Block block, int offset, Endianness endian = DefaultEndianness)
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

        public static void WriteInt(this Block block, int offset, int value, Endianness endian = DefaultEndianness)
        {
            switch (endian)
            {
                case Endianness.Little:
                    block.WriteShort(offset, (short)(value & 0xFFFF));
                    block.WriteShort(offset + 2, (short)((value >> 16) & 0xFFFF));
                    return;

                case Endianness.Big:
                    block.WriteShort(offset + 2, (short)(value & 0xFFFF), Endianness.Big);
                    block.WriteShort(offset, (short)((value >> 16) & 0xFFFF), Endianness.Big);
                    return;

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static uint ReadUInt(this Block block, int offset, Endianness endian = DefaultEndianness)
            => (uint)block.ReadInt(offset, endian);

        public static void WriteUInt(this Block block, int offset, uint value, Endianness endian = DefaultEndianness)
            => block.WriteInt(offset, (int)value, endian);

        public static Block ReadBlock(this Block block, int offset, int count)
            => new Block(block.Data, offset, count);

        public static void WriteBlock(this Block block, int offset, Block toWrite)
            => toWrite.CopyTo(block, offset);

        public static bool ReadBool8(this Block block, int offset)
            => block.ReadByte(offset) != 0;

        public static void WriteBool8(this Block block, int offset, bool value)
            => block.WriteByte(offset, (byte)(value ? 1 : 0));

        public static bool ReadBool16(this Block block, int offset)
            => block.ReadUShort(offset) != 0;

        public static void WriteBool16(this Block block, int offset, bool value)
            => block.WriteUShort(offset, (ushort)(value ? 1 : 0));

        public static bool ReadBool32(this Block block, int offset)
            => block.ReadInt(offset) != 0;

        public static void WriteBool32(this Block block, int offset, bool value)
            => block.WriteInt(offset, value ? 1 : 0);

        public static string ReadString(this Block block, int offset)
        {
            var builder = new StringBuilder();

            byte ch;
            while ((offset < block.Length) && (ch = block.ReadByte(offset++)) != 0)
                builder.Append((char)ch);

            return builder.ToString();
        }

        public static string ReadString(this Block block, int offset, int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentException(nameof(byteCount));

            var builder = new StringBuilder();

            for (int i = 0; i < byteCount; i++)
            {
                byte ch = block.ReadByte(offset++);

                if (ch == 0)
                    break;

                builder.Append((char)ch);
            }

            return builder.ToString();
        }

        public static void WriteString(this Block block, int offset, string value)
        {
            foreach (char ch in value)
                block.WriteByte(offset++, (byte)ch);
        }

        public static void WriteString(this Block block, int offset, string value, int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentException(nameof(byteCount));

            for (int i = 0; i < byteCount; i++)
            {
                if (i < value.Length)
                    block.WriteByte(offset++, (byte)value[i]);
                else
                    block.WriteByte(offset++, 0);
            }
        }
    }

    public enum Endianness
    {
        Little,
        Big
    }
}
