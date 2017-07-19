using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class AllocatableBlock : Block, IAllocatable
    {
        protected IAllocatable _allocator;

        public AllocatableBlock() : base()
        {
            CreateAllocator();
        }

        public AllocatableBlock(Block copyFrom) : base(copyFrom)
        {
            CreateAllocator();
        }

        protected void CreateAllocator()
        {
            _allocator = new RangeAllocator();
        }

        #region IAllocatable implementation

        public int Allocate(int size, int alignment)
            => _allocator.Allocate(size, alignment);

        public void Deallocate(Range range)
            => _allocator.Deallocate(range);

        public int BytesAvailableAt(int offset)
            => _allocator.BytesAvailableAt(offset);

        #endregion
    }
}
