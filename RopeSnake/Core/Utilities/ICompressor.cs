using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public interface ICompressor
    {
        byte[] Compress(byte[] source, int offset, int length);
        byte[] Decompress(byte[] source, int offset);
    }
}
