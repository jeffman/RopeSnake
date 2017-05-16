using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    internal sealed class HashedTile : IEquatable<HashedTile>
    {
        // http://create.stephan-brumme.com/crc32
        private static uint[] Crc32Lookup = new uint[256];
        private const uint Crc32Polynomial = 0xEDB88320;
        private static Dictionary<(bool, bool), Func<Tile, uint>> _hasherLookup;
        private static Dictionary<(bool, bool), Func<Tile, Tile, bool>> _equalsLookup;

        public Tile Tile { get; }
        private bool _flipX;
        private bool _flipY;
        private int _hash;

        static HashedTile()
        {
            ComputeLookup();
            CreateHasherLookups();
        }

        private static void ComputeLookup()
        {
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                    crc = (crc >> 1) ^ (((crc & 1) * 0xFFFFFFFF) & Crc32Polynomial);
                Crc32Lookup[i] = crc;
            }
        }

        private static void CreateHasherLookups()
        {
            _hasherLookup = new Dictionary<(bool, bool), Func<Tile, uint>>();
            _hasherLookup[(false, false)] = HashNoFlip;
            _hasherLookup[(true, false)] = HashFlipX;
            _hasherLookup[(false, true)] = HashFlipY;
            _hasherLookup[(true, true)] = HashFlipBoth;

            _equalsLookup = new Dictionary<(bool, bool), Func<Tile, Tile, bool>>();
            _equalsLookup[(false, false)] = EqualsNoFlip;
            _equalsLookup[(true, false)] = EqualsFlipX;
            _equalsLookup[(false, true)] = EqualsFlipY;
            _equalsLookup[(true, true)] = EqualsFlipBoth;
        }

        public HashedTile(Tile tile, bool flipX, bool flipY)
        {
            _hash = (int)_hasherLookup[(flipX, flipY)](tile);
            Tile = tile;
            _flipX = flipX;
            _flipY = flipY;
        }

        public bool Equals(HashedTile other)
        {
            bool flipX = _flipX ^ other._flipX;
            bool flipY = _flipY ^ other._flipY;
            return _equalsLookup[(flipX, flipY)](Tile, other.Tile);
        }

        public override bool Equals(object obj)
        {
            if (obj is HashedTile other)
                return Equals(other);

            return false;
        }

        public override int GetHashCode()
            => _hash;

        private static uint HashNoFlip(Tile tile)
        {
            uint crc = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    crc = Crc(tile.pixels[x, y], crc);
            }
            return crc;
        }

        private static uint HashFlipX(Tile tile)
        {
            uint crc = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    crc = Crc(tile.pixels[7 - x, y], crc);
            }
            return crc;
        }

        private static uint HashFlipY(Tile tile)
        {
            uint crc = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    crc = Crc(tile.pixels[x, 7 - y], crc);
            }
            return crc;
        }

        private static uint HashFlipBoth(Tile tile)
        {
            uint crc = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    crc = Crc(tile.pixels[7 - x, 7 - y], crc);
            }
            return crc;
        }

        private static bool EqualsNoFlip(Tile first, Tile second)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    if (first.pixels[x, y] != second.pixels[x, y])
                        return false;
            }
            return true;
        }

        private static bool EqualsFlipX(Tile first, Tile second)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    if (first.pixels[x, y] != second.pixels[7 - x, y])
                        return false;
            }
            return true;
        }

        private static bool EqualsFlipY(Tile first, Tile second)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    if (first.pixels[x, y] != second.pixels[x, 7 - y])
                        return false;
            }
            return true;
        }

        private static bool EqualsFlipBoth(Tile first, Tile second)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    if (first.pixels[x, y] != second.pixels[7 - x, 7 - y])
                        return false;
            }
            return true;
        }

        private static uint Crc(byte value, uint previous)
            => (previous >> 8) ^ Crc32Lookup[(previous & 0xFF) ^ value];
    }
}
