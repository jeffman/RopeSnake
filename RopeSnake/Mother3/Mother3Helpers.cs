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
    internal static class Mother3Helpers
    {
        internal const string TextTableKey = "Text.TextTable";

        public static FixedStringTable ReadFixedStringTable(Rom rom, int offset)
        {
            var reader = Mother3TextReader.Create(rom, false, false);
            return rom.ReadFixedStringTable(offset, reader);
        }

        public static void WriteFixedStringTable(Block destination, RomType type, int offset, FixedStringTable table)
        {
            var writer = Mother3TextWriter.Create(destination, type, false, false);
            destination.WriteFixedStringTable(offset, table, writer);
        }

        public static FixedStringTable ReadFixedStringTableFromTextTable(Rom rom, int tableIndex)
        {
            var accessor = new OffsetTableAccessor(rom, rom.GetAsmPointer(TextTableKey));
            return ReadFixedStringTable(rom, accessor.GetEntry(tableIndex).Offset);
        }

        public static List<string> ReadStringTable(Rom rom, int offsetsOffset, int stringsOffset,
            bool isCompressed, bool isEncoded, bool longOffsets)
        {
            var reader = Mother3TextReader.Create(rom, isCompressed, isEncoded);
            return rom.ReadStringTable(offsetsOffset, stringsOffset, reader, longOffsets);
        }

        public static List<string> ReadStringTableFromTextTable(Rom rom, int tableIndex)
        {
            var textTable = new OffsetTableAccessor(rom, rom.GetAsmPointer(TextTableKey));
            return ReadStringTable(rom, textTable.GetEntry(tableIndex).Offset, textTable.GetEntry(tableIndex + 1).Offset, false, false, false);
        }
    }
}
