using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class FixedTable : ITableAccessor<SizedTableEntry>
    {
        public int Count { get; protected set; }
        public int TableOffset { get; protected set; }
        public int EntryLength { get; protected set; }

        public FixedTable(int tableOffset, int entryLength, int count)
        {
            TableOffset = tableOffset;
            EntryLength = entryLength;
            Count = count;
        }

        public SizedTableEntry GetEntry(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            int address = TableOffset + (index * EntryLength);
            return new SizedTableEntry(address, EntryLength);
        }
    }
}
