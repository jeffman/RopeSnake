using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RopeSnake.Core
{
    public sealed class CachedCompressor : ICompressor
    {
        private static HashAlgorithm _hasher;

        private ICompressor _compressor;
        public ConcurrentDictionary<string, byte[]> _cache;
        private object _lockObj = new object();

        static CachedCompressor()
        {
            _hasher = CreateHasher();
        }

        private static HashAlgorithm CreateHasher()
        {
            return SHA1.Create();
        }

        public CachedCompressor(ICompressor compressor)
        {
            _compressor = compressor;
            _cache = new ConcurrentDictionary<string, byte[]>();
        }

        public byte[] Compress(byte[] source, int offset, int length)
        {
            string hash;
            lock (_lockObj)
            {
                hash = string.Join("", _hasher.ComputeHash(source, offset, length).Select(b => b.ToString("x2")));
            }

            byte[] compressed;

            if (_cache.TryGetValue(hash, out compressed))
                return compressed.ToArray();

            compressed = _compressor.Compress(source, offset, length);
            _cache.TryAdd(hash, compressed);
            return compressed;
        }

        public byte[] Decompress(byte[] source, int offset)
            => _compressor.Decompress(source, offset);

        public void ReadCache(string fileName)
        {
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    _cache = new ConcurrentDictionary<string, byte[]>();
                    int count = stream.ReadInt();

                    for (int i = 0; i < count; i++)
                    {
                        string hash = stream.ReadString();
                        int length = stream.ReadInt();
                        byte[] data = stream.ReadBytes(length);

                        _cache.TryAdd(hash, data);
                    }
                }
            }
            catch (Exception ex)
            {
                RLog.Warn($"Error when reading cache.", ex);
                _cache = new ConcurrentDictionary<string, byte[]>();
            }
        }

        public void WriteCache(string fileName)
        {
            var fullPath = Path.GetFullPath(fileName);
            var directory = new DirectoryInfo(Path.GetDirectoryName(fullPath));
            if (!directory.Exists)
                directory.Create();

            using (var stream = File.Create(fileName))
            {
                stream.WriteInt(_cache.Count);

                foreach (var kv in _cache)
                {
                    stream.WriteString(kv.Key);
                    stream.WriteByte(0);

                    stream.WriteInt(kv.Value.Length);
                    stream.WriteBytes(kv.Value);
                }
            }
        }
    }
}
