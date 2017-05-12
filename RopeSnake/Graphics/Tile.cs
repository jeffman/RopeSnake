using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Graphics
{
    public class Tile
    {
        public int Width { get; }
        public int Height { get; }

        internal byte[,] pixels;
        public byte this[int x, int y]
        {
            get => pixels[x, y];
            set => pixels[x, y] = value;
        }

        public Tile(int width, int height)
        {
            pixels = new byte[width, height];
            Width = width;
            Height = height;
        }
    }
}
