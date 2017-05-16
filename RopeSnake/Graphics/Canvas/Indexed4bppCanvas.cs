using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public sealed class Indexed4BppCanvas : Canvas
    {
        public Indexed4BppCanvas(Bitmap image) : base(image, PixelFormat.Format4bppIndexed) { }

        public override unsafe void SetValue(int x, int y, int value)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                return;

            if (value > 15)
                throw new ArgumentException(nameof(value));

            byte* pixels = (byte*)_bitmapData.Scan0 + (x >> 1) + (y * _bitmapData.Stride);
            byte pixel = *pixels;

            if ((x & 1) == 0)
                *pixels = (byte)((pixel & 0xF0) | (value & 0xF));
            else
                *pixels = (byte)((pixel & 0xF) | ((value & 0xF) << 4));
        }

        public override unsafe int GetValue(int x, int y)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                throw new InvalidOperationException($"Coordinate ({x}, {y}) out of bounds");

            byte* pixels = (byte*)_bitmapData.Scan0 + (x >> 1) + (y * _bitmapData.Stride);

            if ((x & 1) == 0)
                return *pixels & 0xF;
            else
                return (*pixels & 0xF0) >> 4;
        }

        public override void SetColor(int x, int y, Color color)
        {
            throw new NotImplementedException();
        }

        public override Color GetColor(int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}
