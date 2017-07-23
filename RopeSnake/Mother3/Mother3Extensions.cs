using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;
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

        public static int GetAsmPointer(this Rom rom, string key)
        {
            var config = Mother3Config.Configs[rom.Type];
            var pointers = new HashSet<int>();

            foreach (var asmPointer in config.AsmPointers[key])
            {
                int pointer = rom.ReadGbaPointer(asmPointer.Location);
                if (pointer > 0)
                    pointer -= asmPointer.TargetOffset;

                pointers.Add(pointer);
            }

            if (pointers.Count > 1)
                throw new Exception($"Differing ASM pointers found for {key}: {String.Join(", ", pointers.Select(p => $"0x{p:X}"))}");

            return pointers.First();
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
            Mother3TextReader reader)
        {
            var strings = new List<string>();

            ushort offset;
            while ((offset = block.ReadUShort(offsetsOffset)) != 0xFFFF)
            {
                strings.Add(reader.ReadString(offset + stringsOffset));
                offsetsOffset += 2;
            }

            return strings;
        }

        public static (int stringsOffset, int totalSize) WriteStringTable(
            this Block block,
            int offset,
            Mother3TextWriter writer,
            IEnumerable<string> strings)
        {
            if (!offset.IsAligned(2))
                throw new ArgumentException("Offset must be aligned by 2");

            // The game follows a convention where the first four bytes at stringsOffset
            // are [FF FF n_low n_high] where n is the string count.

            int count = strings.Count();
            if (count > 0x7FFE)
                throw new Exception("Too many strings");

            int offsetsOffset = offset;
            int stringsOffset = offset + (count * 2);

            int currentOffsetsOffset = offsetsOffset;
            int currentStringsOffset = stringsOffset + 4;

            string prevString = null;
            int prevOffsetOffset = 0;
            bool hasPrevString = false;

            foreach (string str in strings)
            {
                int offsetOffset = 0;

                if (hasPrevString && str == prevString)
                {
                    offsetOffset = prevOffsetOffset;
                }
                else if (IsEmptyString(str))
                {
                    offsetOffset = 0;
                }
                else
                {
                    offsetOffset = currentStringsOffset - stringsOffset;

                    if (offsetOffset > 0xFFFE)
                        throw new Exception("Maximum string table size exceeded");

                    int bytesWritten = writer.WriteString(currentStringsOffset, str);
                    currentStringsOffset += bytesWritten;
                }

                block.WriteUShort(currentOffsetsOffset, (ushort)offsetOffset);
                currentOffsetsOffset += 2;

                prevString = str;
                prevOffsetOffset = offsetOffset;
                hasPrevString = true;
            }

            block.WriteUShort(currentOffsetsOffset, 0xFFFF);
            block.WriteUShort(currentOffsetsOffset + 2, (ushort)count);

            return (stringsOffset, currentStringsOffset - offsetsOffset);
        }

        internal static bool IsEmptyString(string str)
        {
            return String.IsNullOrEmpty(str) ||
                (str.ToLower() == "[end]") ||
                (str.ToLower() == "[ff ff]");
        }
    }
}
