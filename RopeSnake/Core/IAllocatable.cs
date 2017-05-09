using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public interface IAllocatable
    {
        int Allocate(int size, int alignment);
    }

    public sealed class AllocationException : Exception
    {
        public int RequestedSize { get; }
        public int RequestedAlignment { get; }
        public IEnumerable<Range> Ranges { get; }

        public AllocationException(int requestedSize, int requestedAlignment)
            : this(requestedSize, requestedAlignment, null)
        { }

        public AllocationException(int requestedSize, int requestedAlignment, IEnumerable<Range> ranges)
            : this($"Could not allocate the requested size of {requestedSize}. Alignment: {requestedAlignment}",
                  requestedSize,
                  requestedAlignment,
                  ranges)
        { }

        public AllocationException(string message, int requestedSize, int requestedAlignment, IEnumerable<Range> ranges)
            : base(message)
        {
            RequestedSize = requestedSize;
            RequestedAlignment = requestedAlignment;
            Ranges = ranges;
        }
    }
}
