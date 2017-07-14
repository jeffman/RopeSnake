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
            reader.BaseStream.Position = 0x137149C;
            var readString = reader.ReadString();
            Assert.AreEqual("◇だんろのおくに あながあいている。[WAIT FF00]◇おちてみますか?[BREAK][MENU 2]   はい   いいえ[BREAK][ENDMENU]", readString);
        }

        [TestMethod]
        public void ReadSaturnString()
        {
            var reader = Mother3TextReader.Create(origRom, false, false);
            reader.BaseStream.Position = 0x13724BA;
            var readString = reader.ReadString();
            Assert.AreEqual("[ALTFONT]◆のりものて゛す　に[BREAK]　のりこみますか？[WAIT FF00][MENU 2]　　　はい　　　いいえ[BREAK][ENDMENU]", readString);
        }

        [TestMethod]
        public void ReadStringEnglish10()
        {
            var reader = Mother3TextReader.Create(enRom10, true, true);
            reader.BaseStream.Position = 0x1370C08;
            var readString = reader.ReadString();
            Assert.AreEqual("@There's a hole inside the fireplace.[WAIT FF00]@Fall down it?[WAIT FF00][MENU 2]¹Yes²No[ENDMENU]", readString);
        }

        [TestMethod]
        public void ReadSaturnStringEnglish10()
        {
            var reader = Mother3TextReader.Create(enRom10, true, true);
            reader.BaseStream.Position = 0x1371D88;
            var readString = reader.ReadString();
            Assert.AreEqual("[SATURN]@THIS IS[BREAK]  \"THIS VEHICLE\".[WAIT FF00]  YOU RIDE?[WAIT FF00][MENU 2][NORMAL]¹Yes²No[ENDMENU]", readString);
        }
    }
}
