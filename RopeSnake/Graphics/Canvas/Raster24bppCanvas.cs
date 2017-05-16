using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public class Raster24bppCanvas : Canvas
    {
        public Raster24bppCanvas(Bitmap image) : base(image, PixelFormat.Format24bppRgb) { }

        public override unsafe void SetColor(int x, int y, Color color)
            => SetValue(x, y, color.ToArgb() & 0xFFFFFF);

        public override unsafe Color GetColor(int x, int y)
            => Color.FromArgb(GetValue(x, y) | -16777216);

        public override unsafe void SetValue(int x, int y, int value)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                return;

            byte* pixels = (byte*)_bitmapData.Scan0 + (x * 3) + (y * _bitmapData.Stride);
            *pixels++ = (byte)(value & 0xFF);
            *pixels++ = (byte)((value >> 8) & 0xFF);
            *pixels++ = (byte)((value >> 16) & 0xFF);
        }

        public override unsafe int GetValue(int x, int y)
        {
            if (x < 0 || x >= BaseBitmap.Width || y < 0 || y >= BaseBitmap.Height)
                throw new InvalidOperationException($"Coordinate ({x}, {y}) out of bounds");

            byte* pixels = (byte*)_bitmapData.Scan0 + (x * 3) + (y * _bitmapData.Stride);
            int value = *pixels++;
            value |= ((*pixels++) << 8);
            value |= ((*pixels++) << 16);

            return value;
        }
    }
}
