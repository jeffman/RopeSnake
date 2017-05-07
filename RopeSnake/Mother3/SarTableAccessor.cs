using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public sealed class SarTableAccessor : SarTableBase, ITableAccessor<SizedTableEntry>
    {
        public SarTableAccessor(Block source, int tableOffset)
            : base(source, tableOffset)
        { }

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
    }
}
