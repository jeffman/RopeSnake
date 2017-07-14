using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3
{
    public sealed class FixedStringTable
    {
        public int StringLength { get; set; }
        public List<string> Strings { get; set; } = new List<string>();
    }
}
