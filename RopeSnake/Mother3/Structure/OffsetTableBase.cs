using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public abstract class OffsetTableBase
    {
        public int Count { get; }
        public int TableOffset { get; }
        public Block Source { get; set; }

        protected OffsetTableBase(Block source, int tableOffset)
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
    }
}
