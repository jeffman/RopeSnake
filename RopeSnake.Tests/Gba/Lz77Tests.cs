using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Gba;
using RopeSnake.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RopeSnake.Tests.Gba
{
    [TestClass]
    public class Lz77Tests
    {
        [TestMethod]
        public void Compress()
        {
            var decompressed = File.ReadAllBytes("Artifacts\\Mother3\\titlescreen_decompressed.bin");
            var compressed = File.ReadAllBytes("Artifacts\\Mother3\\titlescreen_compressed.bin");

            var compressor = new Lz77Compressor(true);
            var recompressed = compressor.Compress(decompressed, 0, decompressed.Length);

            CollectionAssert.AreEqual(compressed, recompressed);
        }

        [TestMethod]
        public void Decompress()
        {
            var decompressed = File.ReadAllBytes("Artifacts\\Mother3\\titlescreen_decompressed.bin");
            var compressed = File.ReadAllBytes("Artifacts\\Mother3\\titlescreen_compressed.bin");

            var compressor = new Lz77Compressor(true);
            var redecompressed = compressor.Decompress(compressed, 0);

            CollectionAssert.AreEqual(decompressed, redecompressed);
        }
    }
}
