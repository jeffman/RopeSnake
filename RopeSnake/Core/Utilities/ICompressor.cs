using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public interface ICompressor
    {
        Block Compress(Block source, int offset, int length);
        Block Decompress(Block source, int offset);
    }
}
