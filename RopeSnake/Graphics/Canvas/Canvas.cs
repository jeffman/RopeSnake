using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace RopeSnake.Graphics
{
    public abstract class Canvas : IDisposable
    {
        protected internal static readonly PixelFormat[] _indexedFormats = {
            PixelFormat.Format1bppIndexed,
            PixelFormat.Format4bppIndexed,
            PixelFormat.Format8bppIndexed
        };

        public Bitmap BaseBitmap { get; }
        public int Width => BaseBitmap.Width;
        public int Height => BaseBitmap.Height;
        public bool IsIndexed => _indexedFormats.Contains(BaseBitmap.PixelFormat);

        protected BitmapData _bitmapData = null;
        protected bool _disposed;

        protected Canvas(Bitmap image, PixelFormat supportedFormat)
        {
            BaseBitmap = image ?? throw new ArgumentNullException(nameof(image));

            if (image.PixelFormat != supportedFormat)
                throw new InvalidOperationException("Pixel format not supported");

            BeginEdit();
        }

        protected virtual void BeginEdit()
        {
            _bitmapData = BaseBitmap.LockBits(
                new Rectangle(0, 0, BaseBitmap.Width, BaseBitmap.Height),
                ImageLockMode.ReadWrite,
                BaseBitmap.PixelFormat);
        }

        protected virtual void EndEdit()
        {
            BaseBitmap.UnlockBits(_bitmapData);
        }

        public static Canvas Create(Bitmap image)
        {
            switch (image.PixelFormat)
            {
                case PixelFormat.Format4bppIndexed:
                    return new Indexed4BppCanvas(image);

                case PixelFormat.Format8bppIndexed:
                    return new Indexed8bppCanvas(image);

                case PixelFormat.Format24bppRgb:
                    return new Raster24bppCanvas(image);

                case PixelFormat.Format32bppArgb:
                    return new Raster32bppCanvas(image);

                default:
                    throw new InvalidOperationException($"Pixel format not supported: {image.PixelFormat}");
            }
        }

        public abstract void SetColor(int x, int y, Color color);
        public abstract Color GetColor(int x, int y);
        public abstract void SetValue(int x, int y, int value);
        public abstract int GetValue(int x, int y);

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
