using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public class Tilemap : IEnumerable<TileInfo>, IEnumerable<(int, int, TileInfo)>
    {
        public int TileWidth { get; }
        public int TileHeight { get; }
        public int PixelWidth { get; }
        public int PixelHeight { get; }

        internal TileInfo[,] map;
        public TileInfo this[int x, int y]
        {
            get => map[x, y];
            set => map[x, y] = value;
        }

        public Tilemap(int tileWidth, int tileHeight, int pixelWidth, int pixelHeight)
        {
            map = new TileInfo[tileWidth, tileHeight];
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }

        public IEnumerator<TileInfo> GetEnumerator()
        {
            for (int y = 0; y < TileHeight; y++)
            {
                for (int x = 0; x < TileWidth; x++)
                    yield return this[x, y];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<TileInfo>)this).GetEnumerator();

        IEnumerator<(int, int, TileInfo)> IEnumerable<(int, int, TileInfo)>.GetEnumerator()
        {
            for (int y = 0; y < TileHeight; y++)
            {
                for (int x = 0; x < TileWidth; x++)
                    yield return (x, y, this[x, y]);
            }
        }
    }
}
