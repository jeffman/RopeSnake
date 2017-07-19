using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class RangeAllocator : IAllocatable
    {
        private object _lockObj = new object();

        #region Private members

        private class RangeComparer : IComparer<Range>
        {
            public int Compare(Range x, Range y)
            {
                int startCompare = x.Start.CompareTo(y.Start);
                if (startCompare == 0)
                {
                    return x.End.CompareTo(y.End);
                }
                else
                {
                    return startCompare;
                }
            }
        }

        private static IComparer<Range> rangeComparer = new RangeComparer();

        private LinkedList<Range> _rangeList;

        #endregion

        public IEnumerable<Range> Ranges => _rangeList;

        /// <summary>
        /// Gets or sets a flag indicating whether unaligned chunks should be discarded during allocation.
        /// 
        /// If <c>true</c>, unaligned chunks will be discarded during allocation. This results in lower
        /// fragmentation and consequently faster future allocations, but with memory being lost.
        /// 
        /// If <c>false</c>, unaligned chunks will not be discarded, resulting in fragmentation
        /// with no lost memory.
        /// </summary>
        public bool DiscardUnalignedChunks { get; set; } = true;

        // TODO: store "lost" chunks somewhere so that they can be restored/exported later

        public RangeAllocator()
        {
            _rangeList = new LinkedList<Range>();
        }

        public RangeAllocator(IEnumerable<Range> ranges) : this()
        {
            foreach (var range in ranges.Distinct().OrderBy(r => r, rangeComparer))
                _rangeList.AddLast(range);

            Consolidate();
        }

        internal RangeAllocator(params Range[] ranges) : this((IEnumerable<Range>)ranges) { }

        public int Allocate(int size, int alignment)
        {
            if (size < 1)
                throw new ArgumentException(nameof(size));

            if (alignment < 1)
                throw new ArgumentException(nameof(size));

            lock (_lockObj)
            {
                RLog.Debug($"Allocating {size} bytes (alignment {alignment})...");

                var query = _rangeList.EnumerateNodes()
                    .Where(n => n.Value.GetAlignedSize(alignment) >= size)
                    .OrderBy(n => n.Value.Size);

                var match = query.FirstOrDefault();

                if (match == null)
                    throw new AllocationException(size, alignment, Ranges);

                Range range = match.Value;
                int location = range.Start.Align(alignment);

                if (!DiscardUnalignedChunks && location > range.Start)
                {
                    // Add a small chunk for the unaligned portion of the matched range
                    Range before = Range.StartEnd(range.Start, location - 1);
                    _rangeList.AddBefore(match, before);
                }

                if (location + size - 1 < range.End)
                {
                    // Add a chunk for the remaining range
                    Range after = Range.StartEnd(location + size, range.End);
                    _rangeList.AddAfter(match, after);
                }

                _rangeList.Remove(match);

                return location;
            }
        }

        public void Deallocate(Range range)
        {
            lock (_lockObj)
            {
                RLog.Debug($"Deallocating range {range}");

                LinkedListNode<Range> current = _rangeList.First;
                int? compareResult = null;

                while (current != null && (compareResult = rangeComparer.Compare(range, current.Value)) >= 0)
                {
                    current = current.Next;
                }

                if (compareResult == 0)
                {
                    // Ignore duplicates
                    return;
                }

                // Insert
                if (current == null)
                    current = _rangeList.AddLast(range);
                else
                    current = _rangeList.AddBefore(current, range);

                // Consolidate backwards
                Consolidate(current, n => n.Previous);

                // Consolidate forwards
                Consolidate(current, n => n.Next);
            }
        }

        public void Clear()
        {
            _rangeList.Clear();
        }

        public int BytesAvailableAt(int offset)
        {
            foreach (var range in _rangeList)
            {
                if (range.Start <= offset && range.End >= offset)
                    return range.End - offset + 1;
            }

            return 0;
        }

        private void Consolidate()
        {
            if (_rangeList.Count < 1)
                return;

            LinkedListNode<Range> current = _rangeList.First;

            while (current != null)
            {
                current = Consolidate(current, n => n.Next);
            }
        }

        private LinkedListNode<Range> Consolidate(LinkedListNode<Range> current, Func<LinkedListNode<Range>, LinkedListNode<Range>> nextSelector)
        {
            LinkedListNode<Range> next = nextSelector(current);
            while (next != null && current.Value.CanCombineWith(next.Value))
            {
                Range combined = current.Value.CombineWith(next.Value);
                current.Value = combined;
                _rangeList.Remove(next);
                next = nextSelector(current);
            }
            return next;
        }

        public override string ToString()
        {
            return string.Join(", ", _rangeList);
        }
    }
}

