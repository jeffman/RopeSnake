using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class SizedTableEntry : TableEntry, IEquatable<SizedTableEntry>
    {
        public int Size { get; protected set; }

        public SizedTableEntry(int address, int size)
            : base(address)
        {
            Size = size;
        }

        public bool Equals(SizedTableEntry other)
        {
            return (Offset == other.Offset) && (Size == other.Size);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(SizedTableEntry))
                return false;

            return Equals((SizedTableEntry)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Size.GetHashCode();
        }
    }
}
