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
        public int Address { get; protected set; }

        public TableEntry(int address)
        {
            Address = address;
        }

        public bool Equals(TableEntry other)
        {
            return (Address == other.Address);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(TableEntry))
                return false;

            return Equals((TableEntry)obj);
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
    }
}
