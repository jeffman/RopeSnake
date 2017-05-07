using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class StreamExtensionsTests
    {
        private static MethodInfo[] extMethods;
        private Stream stream;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            extMethods = typeof(StreamExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            stream = new MemoryStream();
        }

        [TestMethod]
        public void TestIntegersRoundTrip()
        {
            RoundTripTest<byte>("Byte", 1, false,
                1, byte.MinValue, byte.MaxValue);

            RoundTripTest<sbyte>("SByte", 1, false,
                -1, 0, 1, sbyte.MinValue, sbyte.MaxValue);

            RoundTripTest<short>("Short", 2, true,
                -1, 0, 1, 0xFF, short.MinValue, short.MaxValue);

            RoundTripTest<ushort>("UShort", 2, true,
                1, 0xFF, ushort.MinValue, ushort.MaxValue);

            RoundTripTest("Int", 4, true,
                -1, 0, 1, 0xFF, 0xFFFF, 0xFFFFFF, int.MinValue, int.MaxValue);

            RoundTripTest<uint>("UInt", 4, true,
                1, 0xFF, 0xFFFF, 0xFFFFFF, uint.MinValue, uint.MaxValue);
        }

        private void RoundTripTest<T>(string type, int length, bool doEndian, params T[] values)
        {
            string readerName = (typeof(T) == typeof(byte)) ? "Get" : "Read";
            var readMethods = extMethods.Where(m => m.Name == readerName + type).Take(2);
            var writeMethods = extMethods.Where(m => m.Name == "Write" + type).Take(2);
            var peekMethods = extMethods.Where(m => m.Name == "Peek" + type).Take(2);

            var readMethodDefault = readMethods.First(m => m.GetParameters().Length == 1);
            var readMethodEndian = readMethods.FirstOrDefault(m => m.GetParameters().Length == 2);
            var writeMethodDefault = writeMethods.FirstOrDefault(m => m.GetParameters().Length == 2);
            var writeMethodEndian = writeMethods.FirstOrDefault(m => m.GetParameters().Length == 3);
            var peekMethodDefault = peekMethods.First(m => m.GetParameters().Length == 1);
            var peekMethodEndian = peekMethods.FirstOrDefault(m => m.GetParameters().Length == 2);

            long position = stream.Position;

            foreach (var value in values)
            {
                string message = $"{type} ({value})";

                if (typeof(T) == typeof(byte))
                    stream.WriteByte(Convert.ToByte(value));
                else
                    writeMethodDefault.Invoke(null, new object[] { stream, value });
                Assert.AreEqual(length, stream.Position, "Write" + message);

                stream.Position = position;
                T peek = (T)peekMethodDefault.Invoke(null, new object[] { stream });
                Assert.AreEqual(position, stream.Position, "Peek" + message);
                Assert.AreEqual(value, peek, "Peek" + message);

                T read = (T)readMethodDefault.Invoke(null, new object[] { stream });
                Assert.AreEqual(length, stream.Position, "Read" + message);
                Assert.AreEqual(value, read, "Read" + message);

                stream.Position = position;

                if (doEndian)
                {
                    foreach (var endian in Enum.GetValues(typeof(Endianness)).Cast<Endianness>())
                    {
                        message = $"{type} ({endian}, {value})";

                        writeMethodEndian.Invoke(null, new object[] { stream, value, endian });
                        Assert.AreEqual(length, stream.Position, "Write" + message);

                        stream.Position = position;
                        peek = (T)peekMethodEndian.Invoke(null, new object[] { stream, endian });
                        Assert.AreEqual(position, stream.Position, "Peek" + message);
                        Assert.AreEqual(value, peek, "Peek" + message);

                        read = (T)readMethodEndian.Invoke(null, new object[] { stream, endian });
                        Assert.AreEqual(length, stream.Position, "Read" + message);
                        Assert.AreEqual(value, read, "Read" + message);

                        stream.Position = position;
                    }
                }
            }
        }

        [TestMethod]
        public void ReadString()
        {
            stream.WriteByte(0x54);
            stream.WriteByte(0x65);
            stream.WriteByte(0x73);
            stream.WriteByte(0x74);
            stream.WriteByte(0x0);
            stream.Position = 0;

            Assert.AreEqual("Test", stream.ReadString());
        }

        [TestMethod]
        public void WriteString()
        {
            stream.WriteString("Test");
            Assert.AreEqual(5, stream.Position);

            stream.Position = 0;
            Assert.AreEqual("Test", stream.ReadString());
        }

        [TestMethod]
        public void ReadStringNoNull()
        {
            stream.WriteByte(0x54);
            stream.WriteByte(0x65);
            stream.WriteByte(0x73);
            stream.WriteByte(0x74);
            stream.Position = 0;

            Assert.AreEqual("Test", stream.ReadString());
        }

        [TestMethod]
        public void WriteStringWithNull()
        {
            stream.WriteString("Te\0st");
            Assert.AreEqual(6, stream.Position);

            stream.Position = 0;
            Assert.AreEqual("Te", stream.ReadString());
        }

        [TestMethod]
        public void ReadFixedString()
        {
            stream.WriteByte(0x54);
            stream.WriteByte(0x65);
            stream.WriteByte(0x73);
            stream.WriteByte(0x74);
            stream.Position = 0;

            Assert.AreEqual("Tes", stream.ReadString(3));
            Assert.AreEqual(3, stream.Position);
        }

        [TestMethod]
        public void ReadFixedStringWithNull()
        {
            stream.WriteByte(0x54);
            stream.WriteByte(0x65);
            stream.WriteByte(0x0);
            stream.WriteByte(0x74);
            stream.Position = 0;

            Assert.AreEqual("Te", stream.ReadString(4));
            Assert.AreEqual(4, stream.Position);
        }

        [TestMethod]
        public void WriteFixedString()
        {
            stream.WriteString("Test", 2);
            Assert.AreEqual(2, stream.Position);

            stream.Position = 0;
            Assert.AreEqual("Te", stream.ReadString(2));
        }

        [TestMethod]
        public void WriteFixedStringPadding()
        {
            // Fill with some 0xFF's first
            stream.WriteInt(-1);
            stream.WriteInt(-1);

            stream.Position = 0;
            stream.WriteString("Test", 6);
            Assert.AreEqual(6, stream.Position);

            stream.Position = 0;
            Assert.AreEqual("Test", stream.ReadString(6));
            Assert.AreEqual(6, stream.Position);

            stream.Position = 4;
            Assert.AreEqual(0, stream.ReadShort());
            Assert.AreEqual(-1, stream.ReadShort());
        }
    }
}
