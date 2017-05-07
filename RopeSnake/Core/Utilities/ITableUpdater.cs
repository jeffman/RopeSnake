using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public interface ITableUpdater<in TEntry> where TEntry : TableEntry
    {
        int Count { get; }
        void UpdateEntry(int index, TEntry entry);
    }
}
