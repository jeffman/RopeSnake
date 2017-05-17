using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public sealed class OffsetTableUpdater : OffsetTableBase, ITableUpdater<TableEntry>
    {
        public OffsetTableUpdater(Block source, int tableOffset)
            : base(source, tableOffset)
        { }

        public void UpdateEntry(int index, TableEntry entry)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            int tableAddress = TableOffset + 4 + (index * 4);

            if (entry == null)
                Source.WriteInt(0, tableAddress);
            else
                Source.WriteInt(entry.Address - TableOffset, tableAddress);
        }
    }
}
