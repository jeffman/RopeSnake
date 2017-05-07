﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class TableAccessorExtensionsTests
    {
        private static Block testBlock;
        private static uint[] values;
        private ITableAccessor<TableEntry> accessor;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            values = new uint[]
            {
                0x03020100,
                0x07060504,
                0x83828180,
                0xA7A6A5A4
            };

            testBlock = new Block(values.Length * 4);
            for (int i = 0; i < 4; i++)
                testBlock.WriteUInt(values[i], i * 4);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            accessor = new FixedTableAccessor(0, 4, values.Length);
        }

        [TestMethod]
        public void GetEntries()
        {
            int i = 0;
            foreach (var entry in accessor.GetEntries())
            {
                Assert.AreEqual(i * 4, entry.Address);
                i++;
            }
        }

        [TestMethod]
        public void ParseEntry()
        {
            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], accessor.ParseEntry(i, testBlock, StreamExtensions.ReadUInt));
            }
        }

        [TestMethod]
        public void ParseEntries()
        {
            CollectionAssert.AreEqual(values, accessor.ParseEntries(testBlock, StreamExtensions.ReadUInt).ToList());
        }
    }
}
