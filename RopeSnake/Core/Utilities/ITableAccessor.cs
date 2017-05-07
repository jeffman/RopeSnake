using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public interface ITableAccessor<out TEntry> where TEntry : TableEntry
    {
        int Count { get; }
        TEntry GetEntry(int index);
    }

    public sealed class TableException : Exception
    {
        public TableException(string message) : base(message) { }
    }
}
