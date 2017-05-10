using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class RomTypeDetectorTests
    {
        [TestMethod]
        public void DetectMother3()
        {
            var expected = new Dictionary<string, RomType>
            {
                ["m3_orig.gba"] = new RomType("Mother 3", "jp"),
                ["m3_v10.gba"] = new RomType("Mother 3", "en-v10"),
                ["m3_v11.gba"] = new RomType("Mother 3", "en-v11"),
                ["m3_v12.gba"] = new RomType("Mother 3", "en-v12")
            };

            var rom = new Block();

            foreach (var kv in expected)
            {
                rom.ReadFromFile($"Artifacts\\Roms\\{kv.Key}");
                var type = RomTypeDetector.Detect(rom);
                Assert.AreEqual(kv.Value, type);
            }
        }
    }
}
