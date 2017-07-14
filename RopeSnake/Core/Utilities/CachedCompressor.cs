using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cache = System.Collections.Generic.Dictionary<string, RopeSnake.Core.Block>;

namespace RopeSnake.Core
{
    public sealed class CachedCompressor : ICompressor
    {
        private static HashAlgorithm _hasher;
        private static object _hasherLock = new object();
        private static Dictionary<string, Cache> _globalCache;
        private static object _cacheLock = new object();
        private const string FileExtension = "cache";

        internal ICompressor _compressor;
        private Cache _cache;

        static CachedCompressor()
        {
            _hasher = CreateHasher();
            _globalCache = new Dictionary<string, Cache>(StringComparer.OrdinalIgnoreCase);
        }

        private static HashAlgorithm CreateHasher()
        {
            return SHA1.Create();
        }

        public CachedCompressor(ICompressor compressor, string cacheKey)
        {
            _compressor = compressor;

            lock (_cacheLock)
            {
                if (_globalCache.ContainsKey(cacheKey))
                {
                    _cache = _globalCache[cacheKey];
                }
                else
                {
                    _cache = CreateBlankCache();
                    _globalCache.Add(cacheKey, _cache);
                }
            }
        }

        public Block Compress(Block source, int offset, int length)
        {
            string hash;
            lock (_hasherLock)
            {
                hash = string.Join("", _hasher.ComputeHash(source.Data, offset, length).Select(b => b.ToString("x2")));
            }

            Block compressed;

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(hash, out compressed))
                    return new Block(compressed);
            }

            compressed = _compressor.Compress(source, offset, length);

            lock (_cacheLock)
            {
                _cache.Add(hash, compressed);
            }

            return compressed;
        }

        public Block Decompress(Block source, int offset)
            => _compressor.Decompress(source, offset);

        public static void ReadGlobalCache(string directory)
        {
            lock (_cacheLock)
            {
                var directoryInfo = new DirectoryInfo(directory);
                if (!directoryInfo.Exists)
                {
                    RLog.Warn($"Cache directory does not exist: {directoryInfo.FullName}");
                    return;
                }

                foreach (var cacheFile in directoryInfo.GetFiles($"*.{FileExtension}"))
                {
                    string cacheKey = Path.GetFileNameWithoutExtension(cacheFile.Name);
                    var cache = ReadCache(cacheFile.FullName);
                    MergeCache(cacheKey, cache);
                }
            }
        }

        public static void WriteGlobalCache(string directory)
        {
            lock (_cacheLock)
            {
                foreach (var cache in _globalCache)
                    WriteCache(cache.Value, Path.Combine(directory, $"{cache.Key}.{FileExtension}"));
            }
        }

        internal static void MergeCache(string key, Cache cache)
        {
            if (_globalCache.TryGetValue(key, out Cache existing))
            {
                foreach (var kv in cache)
                    existing[kv.Key] = kv.Value;
            }
            else
            {
                _globalCache.Add(key, cache);
            }
        }

        internal static Cache ReadCache(string fileName)
        {
            try
            {
                var cache = CreateBlankCache();

                using (var stream = new BinaryReader(File.OpenRead(fileName), Encoding.ASCII))
                {
                    int count = stream.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        string hash = stream.ReadString();
                        int length = stream.ReadInt32();
                        Block block = new Block(length);
                        stream.Read(block.Data, 0, length);

                        cache.Add(hash, block);
                    }
                }

                return cache;
            }
            catch (Exception ex)
            {
                RLog.Warn($"Error reading cache.", ex);
                return CreateBlankCache(); // don't return a partially initialized cache
            }
        }

        internal static void WriteCache(Cache cache, string fileName)
        {
            var fullPath = Path.GetFullPath(fileName);
            var directory = new DirectoryInfo(Path.GetDirectoryName(fullPath));
            if (!directory.Exists)
                directory.Create();

            using (var stream = new BinaryWriter(File.Create(fileName), Encoding.ASCII))
            {
                stream.Write(cache.Count);

                foreach (var kv in cache)
                {
                    stream.Write(kv.Key);
                    stream.Write(kv.Value.Length);
                    stream.Write(kv.Value.Data);
                }
            }
        }

        internal static Cache CreateBlankCache()
            => new Cache(StringComparer.OrdinalIgnoreCase);
    }
}
