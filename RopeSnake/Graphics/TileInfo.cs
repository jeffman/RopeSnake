using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public struct TileInfo
    {
        public int Tile { get; }
        public int Palette { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }

        public TileInfo(int tile, int palette, bool flipX, bool flipY)
        {
            Tile = tile;
            Palette = palette;
            FlipX = flipX;
            FlipY = flipY;
        }
    }
}
