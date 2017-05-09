using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class AllocationResult
    {
        public IReadOnlyDictionary<string, int> Allocations { get; }

        public AllocationResult(IDictionary<string, int> allocations)
        {
            Allocations = new ReadOnlyDictionary<string, int>(allocations);
        }
    }
}
