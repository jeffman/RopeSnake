using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class Rom : AllocatableBlock
    {
        public RomType Type { get; private set; }

        public Rom() : base() { }
    }
}
