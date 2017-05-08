using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class RangeAllocatorTests
    {
        [TestMethod]
        public void Create()
        {
            var allocator = new RangeAllocator(Range.StartSize(0, 16));
            Assert.AreEqual("[0, 15]", allocator.ToString());
        }

        [TestMethod]
        public void Allocate()
        {
            var allocator = new RangeAllocator(Range.StartSize(0, 16));
            int allocated = allocator.Allocate(8, 1);
            Assert.AreEqual(0, allocated);
            Assert.AreEqual("[8, 15]", allocator.ToString());
        }

        [TestMethod]
        public void AllocateAlignedDiscard()
        {
            var allocator = new RangeAllocator(Range.StartSize(1, 15));
            int allocated = allocator.Allocate(8, 4);
            Assert.AreEqual(4, allocated);
            Assert.AreEqual("[12, 15]", allocator.ToString());
        }

        [TestMethod]
        public void AllocateNotEnoughSpace()
        {
            var allocator = new RangeAllocator(Range.StartSize(0, 16));
            var expectedException = new AllocationException(32, 1);
            Assert.ThrowsException<AllocationException>(() => allocator.Allocate(32, 1), expectedException.Message);
        }

        [TestMethod]
        public void AllocateAlignedNotEnoughSpace()
        {
            var allocator = new RangeAllocator(Range.StartSize(3, 8));
            var expectedException = new AllocationException(8, 4);
            Assert.ThrowsException<AllocationException>(() => allocator.Allocate(8, 4), expectedException.Message);
        }

        [TestMethod]
        public void AllocateAlignedNoDiscard()
        {
            var allocator = new RangeAllocator(Range.StartSize(1, 15));
            allocator.DiscardUnalignedChunks = false;
            int allocated = allocator.Allocate(8, 4);
            Assert.AreEqual(4, allocated);
            Assert.AreEqual("[1, 3], [12, 15]", allocator.ToString());
        }

        [TestMethod]
        public void Deallocate()
        {
            var allocator = new RangeAllocator();
            allocator.Deallocate(Range.StartEnd(4, 15));

            Assert.AreEqual("[4, 15]", allocator.ToString());

            var range2 = Range.StartEnd(8, 23);
            allocator.Deallocate(range2);

            Assert.AreEqual("[4, 23]", allocator.ToString());
        }
    }
}
