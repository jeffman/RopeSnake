using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RopeSnake.Core;
using RopeSnake.Mother3.Text;

namespace RopeSnake.Mother3
{
    public static class Mother3Extensions
    {
        public static FixedStringTable ReadFixedStringTable(this Stream stream, Mother3TextReader reader)
        {
            int stringLength = stream.ReadUShort();
            int count = stream.ReadUShort();

            var table = new FixedStringTable { StringLength = stringLength };
            reader.BaseStream.Position = stream.Position;

            for (int i = 0; i < count; i++)
            {
                table.Strings.Add(reader.ReadString(stringLength * 2));
            }

            return table;
        }

        public static void WriteFixedStringTable(this Stream stream, FixedStringTable table, Mother3TextWriter writer)
        {
            stream.WriteUShort((ushort)table.StringLength);
            stream.WriteUShort((ushort)table.Strings.Count);
            writer.BaseStream.Position = stream.Position;

            foreach (string str in table.Strings)
                writer.WriteString(str, table.StringLength * 2);
        }

        public static List<string> ReadStringTable(this Stream stream, Mother3TextReader reader,
            int stringsPointer, bool longOffsets = false)
        {
            var strings = new List<string>();
            var offsets = new List<int>();

            ushort offset;
            while ((offset = stream.ReadUShort()) != 0xFFFF)
            {
                if (longOffsets)
                    offset *= 2;

                offsets.Add(offset + stringsPointer);
            }

            foreach (var stringOffset in offsets)
            {
                reader.BaseStream.Position = stringOffset;
                strings.Add(reader.ReadString());
            }

            return strings;
        }
    }
}
