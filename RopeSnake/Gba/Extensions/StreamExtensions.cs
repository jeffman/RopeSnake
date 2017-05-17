using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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

        public static int ReadPointer(this Stream stream)
            => stream.ReadInt().ToPointer();

        public static int FromPointer(this int value)
        {
            if (value == 0)
                return 0;

            if (value < 0x8000000 || value > 0x9FFFFFF)
                RLog.Warn($"Invalid pointer: 0x{value:X}");

            return value & 0x7FFFFFF;
        }

        public static void WritePointer(this Stream stream, int value)
            => stream.WriteInt(value.ToPointer());

        public static int ToPointer(this int value)
        {
            if (value <= 0)
                return 0;

            return value | 0x8000000;
        }

        public static T ReadCompressed<T>(this Stream stream, Func<Stream, int, T> reader)
        {
            var compressor = Compressors.CreateLz77(true);
            var decompressed = compressor.Decompress(stream);
            var decompStream = decompressed.ToStream();
            return reader(decompStream, decompressed.Length);
        }

        public static T ReadCompressed<T>(this Stream stream, Func<Stream, T> reader)
            => stream.ReadCompressed((s, i) => reader(s));

        public static void WriteCompressed(this Stream stream, Action<Stream> writer)
        {
            var buffer = _writeCompressedBuffers.Value;
            var compStream = buffer.ToStream();
            writer(compStream);

            int length = (int)compStream.Position;
            var compressor = Compressors.CreateLz77(true);
            var compressed = compressor.Compress(buffer, 0, length);
            stream.WriteBytes(compressed.Data);
        }

        public static Color ReadColor(this Stream stream)
        {
            int value = stream.ReadUShort();
            int r = value & 0x1F;
            int g = (value >> 5) & 0x1F;
            int b = (value >> 10) & 0x1F;
            return Color.FromArgb(r * 8, g * 8, b * 8);
        }

        public static void WriteColor(this Stream stream, Color color)
        {
            int value = 0;
            value |= ((color.R / 8) & 0x1F);
            value |= ((color.G / 8) & 0x1F) << 5;
            value |= ((color.B / 8) & 0x1F) << 10;
            stream.WriteUShort((ushort)value);
        }

        public static Palette ReadPalette(this Stream stream, int numPalettes, int numColors)
        {
            if (numColors < 0)
                throw new ArgumentException(nameof(numColors));

            var palette = new Palette(numPalettes, numColors);

            for (int i = 0; i < palette.TotalCount; i++)
                palette.SetColor(i, stream.ReadColor());

            return palette;
        }

        public static void WritePalette(this Stream stream, Palette palette)
        {
            if (palette == null)
                throw new ArgumentException(nameof(palette));

            for (int i = 0; i < palette.TotalCount; i++)
                stream.WriteColor(palette.GetColor(i));
        }

        public static Tile ReadTile(this Stream stream, int bitDepth = 4)
        {
            var tile = new Tile(8, 8);

            switch (bitDepth)
            {
                case 4:
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x += 2)
                        {
                            byte pair = stream.GetByte();
                            tile[x, y] = (byte)(pair & 0xF);
                            tile[x + 1, y] = (byte)((pair >> 4) & 0xF);
                        }
                    }
                    break;

                case 8:
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                            tile[x, y] = stream.GetByte();
                    }
                    break;

                default:
                    throw new ArgumentException(nameof(bitDepth));
            }

            return tile;
        }

        public static void WriteTile(this Stream stream, Tile tile, int bitDepth = 4)
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
                            stream.WriteByte(pair);
                        }
                    }
                    break;

                case 8:
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                            stream.WriteByte(tile[x, y]);
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

        public static List<Tile> ReadTileset(this Stream stream, int tileCount, int bitDepth = 4)
        {
            if (tileCount < 0)
                throw new ArgumentException(nameof(tileCount));

            var tileset = new List<Tile>(tileCount);

            for (int i = 0; i < tileCount; i++)
                tileset.Add(stream.ReadTile(bitDepth));

            return tileset;
        }

        public static void WriteTileset(this Stream stream, IEnumerable<Tile> tileset, int bitDepth = 4)
        {
            if (tileset == null)
                throw new ArgumentNullException(nameof(tileset));

            foreach (var tile in tileset)
                stream.WriteTile(tile, bitDepth);
        }

        public static TileInfo ReadTileInfo(this Stream stream)
        {
            int value = stream.ReadUShort();
            int tile = value & 0x3FF;
            bool flipX = (tile & 0x400) != 0;
            bool flipY = (tile & 0x800) != 0;
            int palette = (value >> 12) & 0xF;
            return new TileInfo(tile, palette, flipX, flipY);
        }

        public static void WriteTileInfo(this Stream stream, TileInfo tileInfo)
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

            stream.WriteUShort((ushort)value);
        }

        public static Tilemap ReadTilemap(this Stream stream, int width, int height)
        {
            var tilemap = new Tilemap(width, height, 8, 8);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    tilemap[x, y] = stream.ReadTileInfo();
            }
            return tilemap;
        }

        public static void WriteTilemap(this Stream stream, Tilemap tilemap)
        {
            if (tilemap == null)
                throw new ArgumentNullException(nameof(tilemap));

            for (int y = 0; y < tilemap.TileHeight; y++)
            {
                for (int x = 0; x < tilemap.TileWidth; x++)
                    stream.WriteTileInfo(tilemap[x, y]);
            }
        }
    }
}
