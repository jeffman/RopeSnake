using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public class OffsetTable : ITableUpdater<TableEntry>, ITableAccessor<TableEntry>
    {
        public int Count { get; }
        public int TableOffset { get; }
        public Block Source { get; set; }

        public OffsetTable(Block source, int tableOffset)
        {
            int count = source.ReadInt(tableOffset);
            if (count < 0)
                throw new TableException($"Negative counts not supported: got {count}");

            Count = count;
            TableOffset = tableOffset;
            Source = source;
        }

        public static int GetTableSize(int count)
            => 4 + ((count + 1) * 4);

        public static void CreateNew(Block block, int newOffset, int newCount)
        {
            if (newCount < 0)
                throw new TableException($"Negative counts not supported: given {newCount}");

            block.WriteString(newOffset, "sar ", 4);
            block.WriteInt(newOffset + 4, newCount);
        }

        public TableEntry GetEntry(int index)
        {
            // index == Count is allowed because offset tables contain an extra entry at the end
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            int offset = Source.ReadInt(TableOffset + 4 + (index * 4));

            if (offset == 0)
                return null;

            int address = TableOffset + offset;
            return new TableEntry(address);
        }

        public void UpdateEntry(int index, TableEntry entry)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            int tableAddress = TableOffset + 4 + (index * 4);

            if (entry == null)
                Source.WriteInt(0, tableAddress);
            else
                Source.WriteInt(entry.Offset - TableOffset, tableAddress);
        }
    }
}
