using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;
using RopeSnake.Mother3;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RopeSnake.Tests.Mother3
{
    [TestClass]
    public class Mother3TableTests
    {
        private static Block baseRom;
        private static Block rom;
        private static SizedTableEntry[] expectedSar;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            baseRom = new Block(File.ReadAllBytes("Artifacts/Roms/m3_orig.gba"));
            rom = new Block();
            expectedSar = JsonConvert.DeserializeObject<SizedTableEntry[]>(
                File.ReadAllText("Artifacts/Mother3/orig_sartable_expected.json"));
        }

        [TestInitialize]
        public void TestInitialize()
        {
            baseRom.CopyTo(rom);
        }

        [TestMethod]
        public void ReadSar()
        {
            var accessor = new SarTable(rom, 0x1C90960);

            Assert.AreEqual(expectedSar.Length, accessor.Count);
            CollectionAssert.AreEqual(expectedSar, accessor.GetEntries().ToList());
            Assert.ThrowsException<IndexOutOfRangeException>(() => accessor.GetEntry(-1));
        }

        [TestMethod]
        [ExpectedException(typeof(TableException))]
        public void ReadInvalidSarHeader()
        {
            var accessor = new SarTable(rom, 0);
        }

        [TestMethod]
        public void WriteSar()
        {
            var updater = new SarTable(rom, 0x1C90960);

            Assert.AreEqual(expectedSar.Length, updater.Count);

            for (int i = 0; i < expectedSar.Length; i++)
            {
                updater.UpdateEntry(i, expectedSar[i]);
                Assert.AreEqual(expectedSar[i].Offset - updater.TableOffset, rom.ReadInt(updater.TableOffset + 8 + (i * 8)));
                Assert.AreEqual(expectedSar[i].Size, rom.ReadInt(updater.TableOffset + 12 + (i * 8)));
            }
        }
    }
}
