using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Gba;
using RopeSnake.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;

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
            var recompressed = compressor.Compress(new Block(decompressed), 0, decompressed.Length);

            CollectionAssert.AreEqual(compressed, recompressed.Data);
        }

        [TestMethod]
        public void Decompress()
        {
            var decompressed = File.ReadAllBytes("Artifacts\\Mother3\\titlescreen_decompressed.bin");
            var compressed = File.ReadAllBytes("Artifacts\\Mother3\\titlescreen_compressed.bin");

            var compressor = new Lz77Compressor(true);
            var redecompressed = compressor.Decompress(new Block(compressed), 0);

            CollectionAssert.AreEqual(decompressed, redecompressed.Data);
        }

        [TestMethod]
        public void ReadCache()
        {
            Config.Settings.CacheLz77 = true;
            Compressors.ReadGlobalCache("Artifacts\\CompCache");
            var cachedCompressor = Compressors.CreateLz77(true);
            var ordinaryCompressor = new Mock<Lz77Compressor>(true);

            bool didCacheHit = true;
            ordinaryCompressor
                .Setup(c => c.Compress(
                    It.IsAny<Block>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Callback(() => didCacheHit = false);

            ((CachedCompressor)cachedCompressor)._compressor = ordinaryCompressor.Object;

            var decompressed = new Block(0x1000);
            for (int i = 0; i < decompressed.Length; i++)
                decompressed[i] = (byte)i;

            var compressed = cachedCompressor.Compress(decompressed, 0, decompressed.Length);
            Assert.IsTrue(didCacheHit);
        }

        [TestMethod]
        public void WriteCache()
        {
            Config.Settings.CacheLz77 = true;
            var cachedCompressor = Compressors.CreateLz77(true);

            var decompressed = new Block(0x1000);
            for (int i = 0; i < decompressed.Length; i++)
                decompressed[i] = (byte)i;

            cachedCompressor.Compress(decompressed, 0, decompressed.Length);
            CachedCompressor.WriteGlobalCache("temp\\CompCache");

            string cacheFile = "temp\\CompCache\\lz77.cache";
            Assert.IsTrue(File.Exists(cacheFile));

            byte[] actual = File.ReadAllBytes(cacheFile);
            byte[] expected = File.ReadAllBytes("Artifacts\\CompCache\\lz77.cache");
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
