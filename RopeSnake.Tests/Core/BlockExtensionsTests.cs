using System;
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
            block = new Block(4);
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

        private void RoundTripTest<T>(Action<Block, T, int> writer, Func<Block, int, T> reader,
            int length, params T[] values)
            => RoundTripTest((s, v, o, e) => writer(s, v, o), (s, o, e) => reader(s, o), length, Endianness.Little, values);

        private void RoundTripTest<T>(Action<Block, T, int, Endianness> writer, Func<Block, int, Endianness, T> reader,
            int length, Endianness endian, params T[] values)
        {
            foreach (T value in values)
            {
                writer(block, value, 0, endian);
                T readback = reader(block, 0, endian);
                Assert.AreEqual(value, readback);
            }
        }
    }
}
