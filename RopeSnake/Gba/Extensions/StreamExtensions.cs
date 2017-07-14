using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using RopeSnake.Core;
using RopeSnake.Graphics;
using System.Threading;

namespace RopeSnake.Gba
{
    public static class StreamExtensions
    {
        internal const int _writeCompressedBufferSize = 128 * 1024;
        internal static ThreadLocal<Block> _writeCompressedBuffers;

        static StreamExtensions()
        {
            _writeCompressedBuffers = new ThreadLocal<Block>(() => new Block(_writeCompressedBufferSize));
        }

        public static int ReadGbaPointer(this Block block, int offset)
            => block.ReadInt(offset).FromGbaPointer();

        public static int FromGbaPointer(this int value)
        {
            if (value == 0)
                return 0;

            if (value < 0x8000000 || value > 0x9FFFFFF)
                RLog.Warn($"Invalid pointer: 0x{value:X}");

            return value & 0x7FFFFFF;
        }

        public static void WriteGbaPointer(this Block block, int offset, int value)
            => block.WriteInt(offset, value.ToGbaPointer());

        public static int ToGbaPointer(this int value)
        {
            if (value <= 0)
                return 0;

            return value | 0x8000000;
        }

        public static T ReadCompressed<T>(this Block block, int offset, Func<Block, int, int, T> reader)
        {
            var compressor = Compressors.CreateLz77(true);
            var decompressed = compressor.Decompress(block, offset);
            return reader(decompressed, 0, decompressed.Length);
        }

        public static T ReadCompressed<T>(this Block block, int offset, Func<Block, int, T> reader)
            => block.ReadCompressed(offset, (s, o, l) => reader(s, o));

        public static void WriteCompressed(this Block block, int offset, Func<Block, int, int> writer)
        {
            var buffer = _writeCompressedBuffers.Value;
            int length = writer(buffer, 0);
            var compressor = Compressors.CreateLz77(true);
            var compressed = compressor.Compress(buffer, 0, length);
            block.WriteBlock(offset, compressed);
        }

        public static Color ReadColor(this Block block, int offset)
        {
            int value = block.ReadUShort(offset);
            int r = value & 0x1F;
            int g = (value >> 5) & 0x1F;
            int b = (value >> 10) & 0x1F;
            return Color.FromArgb(r * 8, g * 8, b * 8);
        }

        public static void WriteColor(this Block block, int offset, Color color)
        {
            int value = 0;
            value |= ((color.R / 8) & 0x1F);
            value |= ((color.G / 8) & 0x1F) << 5;
            value |= ((color.B / 8) & 0x1F) << 10;
            block.WriteUShort(offset, (ushort)value);
        }

        public static Palette ReadPalette(this Block block, int offset, int numPalettes, int numColors)
        {
            if (numColors < 0)
                throw new ArgumentException(nameof(numColors));

            var palette = new Palette(numPalettes, numColors);

            for (int i = 0; i < palette.TotalCount; i++)
            {
                palette.SetColor(i, block.ReadColor(offset));
                offset += 2;
            }

            return palette;
        }

        public static void WritePalette(this Block block, int offset, Palette palette)
        {
            if (palette == null)
                throw new ArgumentException(nameof(palette));

            for (int i = 0; i < palette.TotalCount; i++)
            {
                block.WriteColor(offset, palette.GetColor(i));
                offset += 2;
            }
        }

        public static Tile ReadTile(this Block block, int offset, int bitDepth = 4)
        {
            var tile = new Tile(8, 8);

            switch (bitDepth)
            {
                case 4:
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x += 2)
                        {
                            byte pair = block.ReadByte(offset++);
                            tile[x, y] = (byte)(pair & 0xF);
                            tile[x + 1, y] = (byte)((pair >> 4) & 0xF);
                        }
                    }
                    break;

                case 8:
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                            tile[x, y] = block.ReadByte(offset++);
                    }
                    break;

                default:
                    throw new ArgumentException(nameof(bitDepth));
            }

            return tile;
        }

        public static void WriteTile(this Block block, int offset, Tile tile, int bitDepth = 4)
        {
            switch (bitDepth)
            {
                case 4:
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x += 2)
                        {
                            int first = getPixel(x, y, 15);
                            int second = getPixel(x + 1, y, 15);
                            byte pair = (byte)((first & 0xF) | ((second & 0xF) << 4));
                            block.WriteByte(offset, pair);
                        }
                    }
                    break;

                case 8:
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                            block.WriteByte(offset, tile[x, y]);
                    }
                    break;

                default:
                    throw new ArgumentException(nameof(bitDepth));
            }

            byte getPixel(int xx, int yy, int max)
            {
                byte value = tile[xx, yy];
                if (value > max)
                    RLog.Warn($"Pixel value out of range: {value}");
                return value;
            }
        }

        public static List<Tile> ReadTileset(this Block block, int offset, int tileCount, int bitDepth = 4)
        {
            if (tileCount < 0)
                throw new ArgumentException(nameof(tileCount));

            var tileset = new List<Tile>(tileCount);

            for (int i = 0; i < tileCount; i++)
            {
                tileset.Add(block.ReadTile(offset, bitDepth));
                offset += bitDepth * 8;
            }

            return tileset;
        }

        public static void WriteTileset(this Block block, int offset, IEnumerable<Tile> tileset, int bitDepth = 4)
        {
            if (tileset == null)
                throw new ArgumentNullException(nameof(tileset));

            foreach (var tile in tileset)
            {
                block.WriteTile(offset, tile, bitDepth);
                offset += bitDepth * 8;
            }
        }

        public static TileInfo ReadTileInfo(this Block block, int offset)
        {
            int value = block.ReadUShort(offset);
            int tile = value & 0x3FF;
            bool flipX = (tile & 0x400) != 0;
            bool flipY = (tile & 0x800) != 0;
            int palette = (value >> 12) & 0xF;
            return new TileInfo(tile, palette, flipX, flipY);
        }

        public static void WriteTileInfo(this Block block, int offset, TileInfo tileInfo)
        {
            int value = 0;

            if (tileInfo.Tile < 0 || tileInfo.Tile > 0x3FF)
                RLog.Warn($"Invalid tile index: {tileInfo.Tile}");

            if (tileInfo.Palette < 0 || tileInfo.Palette > 15)
                RLog.Warn($"Invalid palette index: {tileInfo.Palette}");

            value |= (tileInfo.Tile & 0x3FF);
            value |= (tileInfo.FlipX ? 0x400 : 0);
            value |= (tileInfo.FlipY ? 0x800 : 0);
            value |= ((tileInfo.Palette & 0xF) << 12);

            block.WriteUShort(offset, (ushort)value);
        }

        public static Tilemap ReadTilemap(this Block block, int offset, int width, int height)
        {
            var tilemap = new Tilemap(width, height, 8, 8);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tilemap[x, y] = block.ReadTileInfo(offset);
                    offset += 2;
                }
            }
            return tilemap;
        }

        public static void WriteTilemap(this Block block, int offset, Tilemap tilemap)
        {
            if (tilemap == null)
                throw new ArgumentNullException(nameof(tilemap));

            for (int y = 0; y < tilemap.TileHeight; y++)
            {
                for (int x = 0; x < tilemap.TileWidth; x++)
                {
                    block.WriteTileInfo(offset, tilemap[x, y]);
                    offset += 2;
                }
            }
        }
    }
}
