using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public static class TableAccessorExtensions
    {
        public static IEnumerable<T> GetEntries<T>(this ITableAccessor<T> accessor)
            where T : TableEntry
        {
            for (int i = 0; i < accessor.Count; i++)
            {
                yield return accessor.GetEntry(i);
            }
        }
    }
}
