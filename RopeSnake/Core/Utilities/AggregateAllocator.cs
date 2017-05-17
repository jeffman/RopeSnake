using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class AggregateAllocator
    {
        public Dictionary<IModule, AllocationResult> Allocate(IDictionary<IModule, CompileResult> compileResults,
            IAllocatable allocator)
        {
            var allocationResults = new Dictionary<IModule, AllocationResult>();

            // Strategy: allocate largest blocks first
            var allocateQuery = compileResults
                .SelectMany(kv => kv.Value.AllocateBlocks.Select(b =>
                    new
                    {
                        Module = kv.Key,
                        KeyBlock = b,
                        Alignment = kv.Value.Alignment
                    }))
                .OrderByDescending(c => c.KeyBlock.Value.Length)
                .Select(c =>
                    new
                    {
                        Module = c.Module,
                        KeyBlock = c.KeyBlock,
                        Address = allocator.Allocate(c.KeyBlock.Value.Length, c.Alignment)
                    });

            foreach (var queryResultGroup in allocateQuery.GroupBy(c => c.Module))
            {
                var allocationResult = new AllocationResult(queryResultGroup
                    .ToDictionary(kv => kv.KeyBlock.Key, kv => kv.Address));

                allocationResults.Add(queryResultGroup.Key, allocationResult);
            }

            return allocationResults;
        }
    }
}
