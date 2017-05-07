﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class Rom : AllocatableBlock
    {
        public Rom(int length) : base(length) { }

        public Rom(byte[] data) : base(data) { }

        public Rom(Block copyFrom) : base(copyFrom) { }
    }
}
