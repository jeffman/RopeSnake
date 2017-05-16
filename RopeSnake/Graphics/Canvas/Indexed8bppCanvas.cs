using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public sealed class Indexed8bppCanvas : Canvas
    {
        public Indexed8bppCanvas(Bitmap image) : base(image, PixelFormat.Format8bppIndexed) { }

        public override void SetColor(int x, int y, Color color)
            => throw new NotImplementedException();

        public override Color GetColor(int x, int y)
            => throw new NotImplementedException();

        public override unsafe void SetValue(int x, int y, int value)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                return;

            byte* pixels = (byte*)_bitmapData.Scan0 + x + (y * _bitmapData.Stride);
            *pixels = (byte)value;
        }

        public override unsafe int GetValue(int x, int y)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                throw new InvalidOperationException($"Coordinate ({x}, {y}) out of bounds");

            byte* pixels = (byte*)_bitmapData.Scan0 + x + (y * _bitmapData.Stride);
            return *pixels;
        }
    }
}
