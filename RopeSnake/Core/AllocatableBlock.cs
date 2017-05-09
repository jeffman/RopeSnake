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
