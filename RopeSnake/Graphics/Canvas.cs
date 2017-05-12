using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public sealed class Canvas : IDisposable
    {
        internal Bitmap _bitmap;
        internal BitmapData _bitmapData = null;
        private bool _disposed;

        public Canvas(Bitmap bitmap)
        {
            _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new InvalidOperationException("Pixel format not supported");

            BeginEdit();
        }

        internal void BeginEdit()
        {
            _bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                ImageLockMode.ReadWrite,
                _bitmap.PixelFormat);
        }

        internal void EndEdit()
        {
            _bitmap.UnlockBits(_bitmapData);
        }

        public unsafe void SetPixel(int x, int y, Color color)
            => SetPixel(x, y, color.ToArgb());

        public unsafe void SetPixel(int x, int y, int color)
        {
            if (x < 0 || x >= _bitmap.Width || y < 0 || y >= _bitmap.Height)
                return;

            int* pixels = (int*)_bitmapData.Scan0 + x + (y * _bitmapData.Stride / 4);
            *pixels = color;
        }

        public unsafe Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= _bitmap.Width || y < 0 || y >= _bitmap.Height)
                throw new InvalidOperationException($"Coordinate ({x}, {y}) out of bounds");

            int* pixels = (int*)_bitmapData.Scan0 + x + (y * _bitmapData.Stride / 4);
            return Color.FromArgb(*pixels);
        }

        #region IDisposable

        ~Canvas()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        internal void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                EndEdit();
                _disposed = true;
            }
        }

        #endregion
    }
}
