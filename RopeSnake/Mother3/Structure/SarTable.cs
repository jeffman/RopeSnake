using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public class SarTable : ITableAccessor<SizedTableEntry>, ITableUpdater<SizedTableEntry>
    {
        public int Count { get; }
        public int TableOffset { get; }
        public Block Source { get; set; }

        public SarTable(Block source, int tableOffset)
        {
            string header = source.ReadString(tableOffset, 4);
            if (header != "sar ")
                throw new TableException($"Expected sar header, but got {header}");

            int count = source.ReadInt(tableOffset + 4);
            if (count < 0)
                throw new TableException($"Negative counts not supported: got {count}");

            Count = count;
            TableOffset = tableOffset;
            Source = source;
        }

        public static int GetTableSize(int count)
            => 8 + (count * 8);

        public static void CreateNew(Block block, int newOffset, int newCount)
        {
            if (newCount < 0)
                throw new TableException($"Negative counts not supported: given {newCount}");

            block.WriteString(newOffset, "sar ", 4);
            block.WriteInt(newOffset + 4, newCount);
        }

        public SizedTableEntry GetEntry(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            int offset = Source.ReadInt(TableOffset + 8 + (index * 8));
            int size = Source.ReadInt(TableOffset + 12 + (index * 8));

            if (offset == 0)
                return null;

            int address = TableOffset + offset;
            return new SizedTableEntry(address, size);
        }

        public void UpdateEntry(int index, SizedTableEntry entry)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            int tableAddress = TableOffset + 8 + (index * 8);

            if (entry == null)
            {
                Source.WriteInt(tableAddress, 0);
                Source.WriteInt(tableAddress + 4, 0);
            }
            else
            {
                Source.WriteInt(tableAddress, entry.Offset - TableOffset);
                Source.WriteInt(tableAddress + 4, entry.Size);
            }
        }

        public void UpdateCount(int newCount)
        {
            if (newCount < 0)
                throw new TableException($"Negative counts not supported: given {newCount}");

            Source.WriteInt(TableOffset + 4, newCount);
        }
    }
}
