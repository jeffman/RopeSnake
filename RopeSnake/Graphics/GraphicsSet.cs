using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public sealed class GraphicsSet
    {
        public Tilemap Tilemap { get; }
        public List<Tile> Tileset { get; }
        public Palette Palette { get; }

        public GraphicsSet(Tilemap tilemap, List<Tile> tileset, Palette palette)
        {
            Tilemap = tilemap;
            Tileset = tileset;
            Palette = palette;
        }
    }
}
