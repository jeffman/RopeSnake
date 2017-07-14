using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class BlockExtensionsTests
    {
        private Block block;

        [TestInitialize]
        public void TestInitialize()
        {
            block = new Block(8);
        }

        [TestMethod]
        public void SByteRoundTrip()
        {
            RoundTripTest(BlockExtensions.WriteSByte, BlockExtensions.ReadSByte, 1,
                sbyte.MinValue,
                sbyte.MaxValue);
        }

        [TestMethod]
        public void ShortRoundTrip()
        {
            RoundTripTest<short>(BlockExtensions.WriteShort, BlockExtensions.ReadShort, 2, Endianness.Little,
                0,
                -1,
                short.MinValue,
                short.MaxValue,
                0xFF);

            RoundTripTest<short>(BlockExtensions.WriteShort, BlockExtensions.ReadShort, 2, Endianness.Big,
                0,
                -1,
                short.MinValue,
                short.MaxValue,
                0xFF);
        }

        [TestMethod]
        public void UShortRoundTrip()
        {
            RoundTripTest<ushort>(BlockExtensions.WriteUShort, BlockExtensions.ReadUShort, 2, Endianness.Little,
                ushort.MinValue,
                ushort.MaxValue,
                0xFF);

            RoundTripTest<ushort>(BlockExtensions.WriteUShort, BlockExtensions.ReadUShort, 2, Endianness.Big,
                ushort.MinValue,
                ushort.MaxValue,
                0xFF);
        }

        [TestMethod]
        public void IntRoundTrip()
        {
            RoundTripTest(BlockExtensions.WriteInt, BlockExtensions.ReadInt, 4, Endianness.Little,
                0,
                -1,
                int.MinValue,
                int.MaxValue,
                0xFF,
                0xFFFF,
                0xFFFFFF);

            RoundTripTest(BlockExtensions.WriteInt, BlockExtensions.ReadInt, 4, Endianness.Big,
                0,
                -1,
                int.MinValue,
                int.MaxValue,
                0xFF,
                0xFFFF,
                0xFFFFFF);
        }

        [TestMethod]
        public void UIntRoundTrip()
        {
            RoundTripTest<uint>(BlockExtensions.WriteUInt, BlockExtensions.ReadUInt, 4, Endianness.Little,
                uint.MinValue,
                uint.MaxValue,
                0xFF,
                0xFFFF,
                0xFFFFFF);

            RoundTripTest<uint>(BlockExtensions.WriteUInt, BlockExtensions.ReadUInt, 4, Endianness.Big,
                uint.MinValue,
                uint.MaxValue,
                0xFF,
                0xFFFF,
                0xFFFFFF);
        }

        private void RoundTripTest<T>(Action<Block, int, T> writer, Func<Block, int, T> reader,
            int length, params T[] values)
            => RoundTripTest((s, v, o, e) => writer(s, v, o), (s, o, e) => reader(s, o), length, Endianness.Little, values);

        private void RoundTripTest<T>(Action<Block, int, T, Endianness> writer, Func<Block, int, Endianness, T> reader,
            int length, Endianness endian, params T[] values)
        {
            foreach (T value in values)
            {
                writer(block, 0, value, endian);
                T readback = reader(block, 0, endian);
                Assert.AreEqual(value, readback);
            }
        }

        [TestMethod]
        public void ReadString()
        {
            block.WriteByte(0, 0x54);
            block.WriteByte(1, 0x65);
            block.WriteByte(2, 0x73);
            block.WriteByte(3, 0x74);
            block.WriteByte(4, 0x0);
            Assert.AreEqual("Test", block.ReadString(0));
        }

        [TestMethod]
        public void WriteString()
        {
            block.WriteString(0, "Test");
            Assert.AreEqual("Test", block.ReadString(0));
        }

        [TestMethod]
        public void ReadStringNoNull()
        {
            block.WriteByte(0, 0x54);
            block.WriteByte(1, 0x65);
            block.WriteByte(2, 0x73);
            block.WriteByte(3, 0x74);
            Assert.AreEqual("Test", block.ReadString(0));
        }

        [TestMethod]
        public void WriteStringWithNull()
        {
            block.WriteString(0, "Te\0st");
            Assert.AreEqual("Te", block.ReadString(0));
        }

        [TestMethod]
        public void ReadFixedString()
        {
            block.WriteByte(0, 0x54);
            block.WriteByte(1, 0x65);
            block.WriteByte(2, 0x73);
            block.WriteByte(3, 0x74);
            Assert.AreEqual("Tes", block.ReadString(0, 3));
        }

        [TestMethod]
        public void ReadFixedStringWithNull()
        {
            block.WriteByte(0, 0x54);
            block.WriteByte(1, 0x65);
            block.WriteByte(2, 0x0);
            block.WriteByte(3, 0x74);
            Assert.AreEqual("Te", block.ReadString(0, 4));
        }

        [TestMethod]
        public void WriteFixedString()
        {
            block.WriteString(0, "Test", 2);
            Assert.AreEqual("Te", block.ReadString(0, 2));
        }

        [TestMethod]
        public void WriteFixedStringPadding()
        {
            // Fill with some 0xFF's first
            block.WriteInt(0, -1);
            block.WriteInt(4, -1);
            block.WriteString(0, "Test", 6);
            Assert.AreEqual("Test", block.ReadString(0, 6));
            Assert.AreEqual(0, block.ReadShort(4));
            Assert.AreEqual(-1, block.ReadShort(6));
        }

        [TestMethod]
        public void ReadJson()
        {
            var basic = File.OpenRead("Artifacts\\basic.json").ReadJson<Dictionary<string, int>>();
            var expected = new Dictionary<string, int>
            {
                ["Key1"] = 123,
                ["Key2"] = 456
            };

            CollectionAssert.AreEquivalent(expected, basic);

            // Try reading it again to make sure the file handle was released
            basic = File.OpenRead("Artifacts\\basic.json").ReadJson<Dictionary<string, int>>();
            CollectionAssert.AreEquivalent(expected, basic);
        }

        [TestMethod]
        public void WriteJson()
        {
            var basic = new Dictionary<string, int>
            {
                ["Key1"] = 123,
                ["Key2"] = 456
            };

            string expected = @"{
  ""Key1"": 123,
  ""Key2"": 456
}";

            File.OpenWrite("Temp\\basic.json").WriteJson(basic);
            Assert.AreEqual(expected, File.ReadAllText("Temp\\basic.json"));

            // Try writing it again to make sure the file handle was released
            File.OpenWrite("Temp\\basic.json").WriteJson(basic);
            Assert.AreEqual(expected, File.ReadAllText("Temp\\basic.json"));
        }
    }
}
