using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class TableEntry : IEquatable<TableEntry>
    {
        public int Offset { get; protected set; }

        public TableEntry(int offset)
        {
            Offset = offset;
        }

        public bool Equals(TableEntry other)
        {
            return (Offset == other.Offset);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(TableEntry))
                return false;

            return Equals((TableEntry)obj);
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode();
        }

        public static implicit operator TableEntry(int address)
            => new TableEntry(address);
    }
}
