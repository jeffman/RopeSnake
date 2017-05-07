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

        public static bool TryParseEntry<TEntry, TElement>(this ITableAccessor<TEntry> accessor,
            int index, Block source, Func<Stream, TEntry, TElement> parser, out TElement result)
            where TEntry : TableEntry
        {
            TEntry entry = accessor.GetEntry(index);

            if (entry != null)
            {
                var stream = source.ToStream(entry.Address);
                result = parser(stream, entry);
                return true;
            }

            result = default(TElement);
            return false;
        }

        public static bool TryParseEntry<TEntry, TElement>(this ITableAccessor<TEntry> accessor,
            int index, Block source, Func<Stream, TElement> parser, out TElement result)
            where TEntry : TableEntry
            => accessor.TryParseEntry(index, source, (s, e) => parser(s), out result);

        public static TElement ParseEntry<TEntry, TElement>(this ITableAccessor<TEntry> accessor,
            int index, Block source, Func<Stream, TEntry, TElement> parser)
            where TEntry : TableEntry
        {
            if (accessor.TryParseEntry(index, source, parser, out TElement result))
            {
                return result;
            }

            return default(TElement);
        }

        public static TElement ParseEntry<TEntry, TElement>(this ITableAccessor<TEntry> accessor,
            int index, Block source, Func<Stream, TElement> parser)
            where TEntry : TableEntry
            => accessor.ParseEntry(index, source, (s, e) => parser(s));

        public static IEnumerable<TElement> ParseEntries<TEntry, TElement>(this ITableAccessor<TEntry> accessor,
            Block source, Func<Stream, int, TEntry, TElement> parser)
            where TEntry : TableEntry
        {
            var stream = source.ToStream();
            int index = 0;

            foreach (var entry in accessor.GetEntries())
            {
                if (entry != null)
                {
                    stream.Position = entry.Address;
                    yield return parser(stream, index, entry);
                }
                else
                {
                    yield return default(TElement);
                }

                index++;
            }
        }

        public static IEnumerable<TElement> ParseEntries<TEntry, TElement>(this ITableAccessor<TEntry> accessor,
            Block source, Func<Stream, TElement> parser)
            where TEntry : TableEntry
            => accessor.ParseEntries(source, (s, i, e) => parser(s));
    }
}
