using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Gba;

namespace RopeSnake.Core
{
    public static class Compressors
    {
        internal const string Lz77Key = "lz77";
        internal const string CacheDirectory = ".cache";

        internal static void ReadGlobalCache(string directory = CacheDirectory)
        {
            if (Config.Settings.CacheEnabled)
                CachedCompressor.ReadGlobalCache(directory);
        }

        internal static void WriteGlobalCache(string directory = CacheDirectory)
        {
            if (Config.Settings.CacheEnabled)
                CachedCompressor.WriteGlobalCache(directory);
        }

        internal static ICompressor Create(ICompressor compressor)
        {
            return compressor;
        }

        public static ICompressor CreateLz77(bool vram)
        {
            ICompressor compressor = new Lz77Compressor(vram);

            if (Config.Settings.CacheLz77)
                compressor = new CachedCompressor(compressor, Lz77Key);

            return compressor;
        }
    }
}
