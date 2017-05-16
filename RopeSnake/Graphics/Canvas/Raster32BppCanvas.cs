using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public sealed class Raster32bppCanvas : Canvas
    {
        public Raster32bppCanvas(Bitmap image) : base(image, PixelFormat.Format32bppArgb) { }

        public override unsafe void SetColor(int x, int y, Color color)
            => SetValue(x, y, color.ToArgb());

        public override unsafe Color GetColor(int x, int y)
            => Color.FromArgb(GetValue(x, y));

        public override unsafe void SetValue(int x, int y, int value)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                return;

            int* pixels = (int*)_bitmapData.Scan0 + x + (y * _bitmapData.Stride / 4);
            *pixels = value;
        }

        public override unsafe int GetValue(int x, int y)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                throw new InvalidOperationException($"Coordinate ({x}, {y}) out of bounds");

            int* pixels = (int*)_bitmapData.Scan0 + x + (y * _bitmapData.Stride / 4);
            return *pixels;
        }
    }
}
