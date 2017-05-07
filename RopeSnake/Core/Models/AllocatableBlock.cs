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

        public AllocatableBlock(int length) : base(length)
        {
            CreateAllocator();
        }

        public AllocatableBlock(byte[] data) : base(data)
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
        {
            return _allocator.Allocate(size, alignment);
        }

        #endregion
    }
}
