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

        public static FixedStringTable ReadFixedStringTable(Rom rom, int pointer)
        {
            var reader = Mother3TextReader.Create(rom, false, false);
            var stream = rom.ToStream(pointer);
            return stream.ReadFixedStringTable(reader);
        }

        public static void WriteFixedStringTable(FixedStringTable table, Rom rom, int pointer)
        {
            var writer = Mother3TextWriter.Create(rom, false, false);
            rom.ToStream(pointer).WriteFixedStringTable(table, writer);
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
