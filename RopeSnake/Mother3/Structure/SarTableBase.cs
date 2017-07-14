using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public abstract class SarTableBase
    {
        public int Count { get; }
        public int TableOffset { get; }
        public Block Source { get; set; }

        protected SarTableBase(Block source, int tableOffset)
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
            block.WriteInt(newCount, newOffset + 4);
        }
    }
}
