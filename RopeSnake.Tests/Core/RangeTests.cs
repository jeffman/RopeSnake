using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void Combine()
        {
            var a = Range.StartEnd(0, 3);
            var b = Range.StartEnd(2, 7);
            var c = Range.StartEnd(10, 13);
            var d = Range.StartEnd(4, 7);

            Assert.AreEqual(Range.StartEnd(0, 7), a.CombineWith(b));
            Assert.AreEqual(a.CombineWith(b), b.CombineWith(a));
            Assert.AreEqual(true, a.CanCombineWith(b));
            Assert.AreEqual(true, b.CanCombineWith(a));
            Assert.AreEqual(false, a.CanCombineWith(c));
            Assert.AreEqual(true, a.CanCombineWith(d));
            Assert.AreEqual(Range.StartEnd(0, 7), a.CombineWith(d));
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ReversedStartEndThrows()
        {
            Range.StartEnd(1, 0);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void NegativeStartEndThrows()
        {
            Range.StartEnd(-2, -1);
        }

        [TestMethod]
        public void Parse()
        {
            var a = Range.Parse("[2,    5]");
            var b = Range.Parse("[2,5]");

            Assert.AreEqual(2, a.Start);
            Assert.AreEqual(5, a.End);
            Assert.AreEqual(4, a.Size);
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void ParseHex()
        {
            var a = Range.Parse("[0x2, 0xB]");

            Assert.AreEqual(2, a.Start);
            Assert.AreEqual(11, a.End);
        }
    }
}
