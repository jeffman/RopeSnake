using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public static class StreamExtensions
    {
        internal static Endianness DefaultEndianness = Endianness.Little;
        internal static Encoding DefaultEncoding = Encoding.ASCII;

        public static byte[] ReadBytes(this Stream stream, int count)
        {
            var bytes = new byte[count];
            stream.Read(bytes, 0, count);
            return bytes;
        }

        public static void WriteBytes(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// So named because Stream.ReadByte returns an int for some bizarre reason
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte GetByte(this Stream stream)
        {
            int value = stream.ReadByte();
            if (value < 0)
                throw new IOException("Tried getting a byte beyond the end of the stream");

            return (byte)value;
        }

        public static sbyte ReadSByte(this Stream stream)
            => (sbyte)stream.GetByte();

        public static short ReadShort(this Stream stream, Endianness endian)
        {
            switch (endian)
            {
                case Endianness.Little:
                    return (short)(stream.GetByte() | (stream.GetByte() << 8)); ;

                case Endianness.Big:
                    return (short)((stream.GetByte() << 8) | stream.GetByte());

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static short ReadShort(this Stream stream)
            => stream.ReadShort(DefaultEndianness);

        public static ushort ReadUShort(this Stream stream, Endianness endian)
            => (ushort)stream.ReadShort(endian);

        public static ushort ReadUShort(this Stream stream)
            => stream.ReadUShort(DefaultEndianness);

        public static int ReadInt(this Stream stream, Endianness endian)
        {
            switch (endian)
            {
                case Endianness.Little:
                    return stream.ReadUShort() | (stream.ReadUShort() << 16);

                case Endianness.Big:
                    return (stream.ReadUShort(endian) << 16) | stream.ReadUShort(endian);

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static int ReadInt(this Stream stream)
            => stream.ReadInt(DefaultEndianness);

        public static uint ReadUInt(this Stream stream, Endianness endian)
            => (uint)stream.ReadInt(endian);

        public static uint ReadUInt(this Stream stream)
            => stream.ReadUInt(DefaultEndianness);

        public static T Peek<T>(this Stream stream, Func<Stream, T> reader)
        {
            long position = stream.Position;
            T value = reader(stream);
            stream.Position = position;
            return value;
        }

        public static byte PeekByte(this Stream stream)
            => stream.Peek(GetByte);

        public static sbyte PeekSByte(this Stream stream)
            => stream.Peek(ReadSByte);

        public static short PeekShort(this Stream stream, Endianness endian)
            => stream.Peek(s => s.ReadShort(endian));

        public static short PeekShort(this Stream stream)
            => stream.PeekShort(DefaultEndianness);

        public static ushort PeekUShort(this Stream stream, Endianness endian)
            => stream.Peek(s => s.ReadUShort(endian));

        public static ushort PeekUShort(this Stream stream)
            => stream.PeekUShort(DefaultEndianness);

        public static int PeekInt(this Stream stream, Endianness endian)
            => stream.Peek(s => s.ReadInt(endian));

        public static int PeekInt(this Stream stream)
            => stream.PeekInt(DefaultEndianness);

        public static uint PeekUInt(this Stream stream, Endianness endian)
            => stream.Peek(s => s.ReadUInt(endian));

        public static uint PeekUInt(this Stream stream)
            => stream.PeekUInt(DefaultEndianness);

        public static void WriteSByte(this Stream stream, sbyte value)
            => stream.WriteByte((byte)value);

        public static void WriteShort(this Stream stream, short value, Endianness endian)
        {
            switch (endian)
            {
                case Endianness.Little:
                    stream.WriteByte((byte)(value & 0xFF));
                    stream.WriteByte((byte)((value >> 8) & 0xFF));
                    return;

                case Endianness.Big:
                    stream.WriteByte((byte)((value >> 8) & 0xFF));
                    stream.WriteByte((byte)(value & 0xFF));
                    return;

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static void WriteShort(this Stream stream, short value)
            => stream.WriteShort(value, DefaultEndianness);

        public static void WriteUShort(this Stream stream, ushort value, Endianness endian)
            => stream.WriteShort((short)value, endian);

        public static void WriteUShort(this Stream stream, ushort value)
            => stream.WriteUShort(value, DefaultEndianness);

        public static void WriteInt(this Stream stream, int value, Endianness endian)
        {
            switch (endian)
            {
                case Endianness.Little:
                    stream.WriteShort((short)(value & 0xFFFF));
                    stream.WriteShort((short)((value >> 16) & 0xFFFF));
                    return;

                case Endianness.Big:
                    stream.WriteShort((short)((value >> 16) & 0xFFFF), Endianness.Big);
                    stream.WriteShort((short)(value & 0xFFFF), Endianness.Big);
                    return;

                default:
                    throw new Exception("Invalid endianness");
            }
        }

        public static void WriteInt(this Stream stream, int value)
            => stream.WriteInt(value, DefaultEndianness);

        public static void WriteUInt(this Stream stream, uint value, Endianness endian)
            => stream.WriteInt((int)value, endian);

        public static void WriteUInt(this Stream stream, uint value)
            => stream.WriteUInt(value, DefaultEndianness);

        public static string ReadString(this Stream stream)
        {
            var builder = new StringBuilder();

            using (var reader = new StreamReader(stream, DefaultEncoding))
            {
                int c;
                while ((c = reader.Read()) > 0)
                {
                    builder.Append((char)c);
                }
            }

            return builder.ToString();
        }

        public static string ReadString(this Stream stream, int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentException(nameof(byteCount));

            var bytes = stream.ReadBytes(byteCount);
            var str = DefaultEncoding.GetString(bytes);

            return new string(str.TakeWhile(c => c != '\0').ToArray());
        }

        public static string PeekString(this Stream stream)
        {
            long position = stream.Position;
            string str = stream.ReadString();
            stream.Position = position;
            return str;
        }

        public static string PeekString(this Stream stream, int byteCount)
        {
            long position = stream.Position;
            string str = stream.ReadString(byteCount);
            stream.Position = position;
            return str;
        }

        public static void WriteString(this Stream stream, string value)
        {
            using (var writer = new StreamWriter(stream, DefaultEncoding, 4096, true))
            {
                writer.Write(value);
            }
        }

        public static void WriteString(this Stream stream, string value, int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentException(nameof(byteCount));

            var bytes = DefaultEncoding.GetBytes(value);

            if (bytes.Length >= byteCount)
            {
                stream.Write(bytes, 0, byteCount);
            }
            else
            {
                stream.Write(bytes, 0, bytes.Length);
                for (int i = bytes.Length; i < byteCount; i++)
                    stream.WriteByte(0);
            }
        }

        public static Stream At(this Stream stream, int position)
        {
            stream.Position = position;
            return stream;
        }
    }
}
