using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public class SarTableUpdater : SarTableBase, ITableUpdater<SizedTableEntry>
    {
        public SarTableUpdater(Block source, int tableOffset)
            : base(source, tableOffset)
        { }

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
