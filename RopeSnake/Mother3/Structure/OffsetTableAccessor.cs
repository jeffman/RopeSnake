using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public sealed class OffsetTableAccessor : OffsetTableBase, ITableAccessor<TableEntry>
    {
        public OffsetTableAccessor(Block source, int tableOffset)
            : base(source, tableOffset)
        { }

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
    }
}
