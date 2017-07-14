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
            var accessor = new OffsetTableAccessor(rom, Mother3Config.Configs[rom.Type].GetAsmPointer(TextTableKey, rom));
            return ReadFixedStringTable(rom, accessor.GetEntry(tableIndex).Address);
        }

        public static List<string> ReadStringTable(Rom rom, int offsetsPointer, int stringsPointer,
            bool isCompressed, bool isEncoded, bool longOffsets)
        {
            var reader = Mother3TextReader.Create(rom, isCompressed, isEncoded);
            var stream = rom.ToStream(offsetsPointer);
            return stream.ReadStringTable(reader, stringsPointer, longOffsets);
        }

        public static List<string> ReadStringTableFromTextTable(Rom rom, int tableIndex)
        {
            var textTable = new OffsetTableAccessor(rom, Mother3Config.Configs[rom.Type].GetAsmPointer(TextTableKey, rom));
            return ReadStringTable(rom, textTable.GetEntry(tableIndex).Address, textTable.GetEntry(tableIndex + 1).Address, false, false, false);
        }
    }
}
