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
                Source.WriteInt(0, tableAddress);
                Source.WriteInt(0, tableAddress + 4);
            }
            else
            {
                Source.WriteInt(entry.Address - TableOffset, tableAddress);
                Source.WriteInt(entry.Size, tableAddress + 4);
            }
        }
    }
}
