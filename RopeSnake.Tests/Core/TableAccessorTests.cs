using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class TableAccessorTests
    {
        private static Block testBlock;
        private static uint[] values;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            testBlock = new Block(new byte[]
            {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                0x80,
                0x81,
                0x82,
                0x83,
                0xA4,
                0xA5,
                0xA6,
                0xA7
            });

            values = new uint[]
            {
                0x03020100,
                0x07060504,
                0x83828180,
                0xA7A6A5A4
            };
        }

        [TestMethod]
        public void FixedTableTest()
        {
            var accessor = new FixedTableAccessor(0, 4, 4);

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(values[i], testBlock.ReadUInt(accessor.GetEntry(i).Offset));
            }
        }
    }
}
