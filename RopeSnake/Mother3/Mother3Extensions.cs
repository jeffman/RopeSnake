using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Mother3.Text;

namespace RopeSnake.Mother3
{
    public static class Mother3Extensions
    {
        public static FixedStringTable ReadFixedStringTable(this Block block, int offset, Mother3TextReader reader)
        {
            int stringLength = block.ReadUShort(offset);
            int count = block.ReadUShort(offset + 2);

            var table = new FixedStringTable { StringLength = stringLength };
            offset += 4;

            for (int i = 0; i < count; i++)
            {
                table.Strings.Add(reader.ReadString(offset, stringLength * 2));
                offset += stringLength * 2;
            }

            return table;
        }

        public static void WriteFixedStringTable(this Block block, int offset, FixedStringTable table, Mother3TextWriter writer)
        {
            block.WriteUShort(offset, (ushort)table.StringLength);
            block.WriteUShort(offset + 2, (ushort)table.Strings.Count);
            offset += 4;

            foreach (string str in table.Strings)
            {
                writer.WriteString(offset, str, table.StringLength * 2);
                offset += table.StringLength * 2;
            }
        }

        public static List<string> ReadStringTable(
            this Block block,
            int offsetsOffset,
            int stringsOffset,
            Mother3TextReader reader,
            bool longOffsets = false)
        {
            var strings = new List<string>();

            ushort offset;
            while ((offset = block.ReadUShort(offsetsOffset)) != 0xFFFF)
            {
                if (longOffsets)
                    offset *= 2;

                strings.Add(reader.ReadString(offset + stringsOffset));
                offsetsOffset += 2;
            }

            return strings;
        }
    }
}
