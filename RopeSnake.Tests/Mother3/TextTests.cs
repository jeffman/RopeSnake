using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;
using RopeSnake.Mother3;
using RopeSnake.Mother3.Text;

namespace RopeSnake.Tests.Mother3
{
    [TestClass]
    public class TextTests
    {
        static Rom origRom;
        static Rom enRom10;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            origRom = new Rom();
            origRom.ReadFromFile("Artifacts\\Roms\\m3_orig.gba");

            enRom10 = new Rom();
            enRom10.ReadFromFile("Artifacts\\Roms\\m3_v10.gba");
        }

        [TestMethod]
        public void ReadString()
        {
            var reader = Mother3TextReader.Create(origRom, false, false);
            var readString = reader.ReadString(0x137149C);
            Assert.AreEqual("◇だんろのおくに あながあいている。[WAIT FF00]◇おちてみますか?[BREAK][MENU 2]   はい   いいえ[BREAK][ENDMENU]", readString);
        }

        [TestMethod]
        public void ReadSaturnString()
        {
            var reader = Mother3TextReader.Create(origRom, false, false);
            var readString = reader.ReadString(0x13724BA);
            Assert.AreEqual("[ALTFONT]◆のりものて゛す　に[BREAK]　のりこみますか？[WAIT FF00][MENU 2]　　　はい　　　いいえ[BREAK][ENDMENU]", readString);
        }

        [TestMethod]
        public void ReadStringEnglish10()
        {
            var reader = Mother3TextReader.Create(enRom10, true, true);
            var readString = reader.ReadString(0x1370C08);
            Assert.AreEqual("@There's a hole inside the fireplace.[WAIT FF00]@Fall down it?[WAIT FF00][MENU 2]¹Yes²No[ENDMENU]", readString);
        }

        [TestMethod]
        public void ReadSaturnStringEnglish10()
        {
            var reader = Mother3TextReader.Create(enRom10, true, true);
            var readString = reader.ReadString(0x1371D88);
            Assert.AreEqual("[SATURN]@THIS IS[BREAK]  \"THIS VEHICLE\".[WAIT FF00]  YOU RIDE?[WAIT FF00][MENU 2][NORMAL]¹Yes²No[ENDMENU]", readString);
        }

        [TestMethod]
        public void WriteStringTable()
        {
            var block = new Block(0x100);
            var strings = new string[]
            {
                "◇だんろのおくに",
                "あながあいている。[WAIT FF00]◇おちてみますか?",
                "[MENU 2]   はい   いいえ[BREAK][ENDMENU]",
                "[MENU 2]   はい   いいえ[BREAK][ENDMENU]",
                ""
            };

            var writer = Mother3TextWriter.Create(block, origRom.Type, false, false);
            (int stringsOffset, int totalSize) = block.WriteStringTable(32, writer, strings);

            Assert.AreEqual(42, stringsOffset);
            Assert.AreEqual(0x6A, totalSize);

            var expectedBlock = new Block(0x100);
            expectedBlock.ReadFromFile("Artifacts\\Mother3\\stringtable.bin");

            CollectionAssert.AreEqual(expectedBlock.Data, block.Data);
        }
    }
}
