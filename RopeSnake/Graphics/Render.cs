using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RopeSnake.Graphics
{
    public static class Render
    {
        internal delegate void ZeroHandlerDelegate(Canvas canvas, int x, int y, Color color);
        internal static Dictionary<ZeroIndexHandling, ZeroHandlerDelegate> _zeroHandlers;

        static Render()
        {
            _zeroHandlers = new Dictionary<ZeroIndexHandling, ZeroHandlerDelegate>();
            _zeroHandlers[ZeroIndexHandling.DoNothing] = (canvas, x, y, color) => { };
            _zeroHandlers[ZeroIndexHandling.DrawSolid] = (canvas, x, y, color) => { canvas.SetColor(x, y, color); };
            _zeroHandlers[ZeroIndexHandling.DrawTransparent] = (canvas, x, y, color) => { canvas.SetColor(x, y, Color.Transparent); };
        }

        public static void Tilemap(
            Canvas canvas,
            Tilemap tilemap,
            IEnumerable<Tile> tileset,
            Palette palette,
            ZeroIndexHandling zeroHandling = ZeroIndexHandling.DoNothing,
            Region clippingRegion = null)
        {
            if (canvas.IsIndexed)
                AssignPalette(canvas.BaseBitmap, palette, zeroHandling);

            for (int tileY = 0; tileY < tilemap.TileHeight; tileY++)
            {
                for (int tileX = 0; tileX < tilemap.TileWidth; tileX++)
                {
                    var info = tilemap[tileX, tileY];
                    var tile = tileset.ElementAt(info.Tile);

                    var tileRect = new Rectangle(
                        tileX * tilemap.PixelWidth,
                        tileY * tilemap.PixelHeight,
                        tile.Width,
                        tile.Height);

                    if (clippingRegion == null || clippingRegion.IsVisible(tileRect))
                    {
                        Render.Tile(canvas, tileRect.X, tileRect.Y, tile, palette, info, zeroHandling);
                    }
                }
            }
        }

        public static void Tile(
            Canvas canvas,
            int x,
            int y,
            Tile tile,
            Palette palette,
            TileInfo info,
            ZeroIndexHandling zeroHandling)
            => Render.Tile(canvas, x, y, tile, palette, info.Palette, info.FlipX, info.FlipY, zeroHandling);

        public static void Tile(
            Canvas canvas,
            int x,
            int y,
            Tile tile,
            Palette palette,
            int paletteIndex,
            bool flipX,
            bool flipY,
            ZeroIndexHandling zeroHandling)
        {
            if (canvas.IsIndexed)
                Render.IndexedTile(canvas, x, y, tile, paletteIndex * palette.ColorsPerPalette, flipX, flipY, zeroHandling);
            else
                Render.RasterTile(canvas, x, y, tile, palette, paletteIndex, flipX, flipY, zeroHandling);
        }

        public static void RasterTile(
            Canvas canvas,
            int x,
            int y,
            Tile tile,
            Palette palette,
            int paletteIndex,
            bool flipX,
            bool flipY,
            ZeroIndexHandling zeroHandling)
        {
            for (int py = 0; py < tile.Height; py++)
            {
                for (int px = 0; px < tile.Width; px++)
                {
                    int sourceX = flipX ? (tile.Width - px - 1) : px;
                    int sourceY = flipY ? (tile.Height - py - 1) : py;
                    byte index = tile[sourceX, sourceY];

                    if (index == 0)
                        _zeroHandlers[zeroHandling](canvas, x + px, y + py, palette.GetColor(paletteIndex, index));
                    else
                        canvas.SetColor(x + px, y + py, palette.GetColor(paletteIndex, index));
                }
            }
        }

        public static void IndexedTile(
            Canvas canvas,
            int x,
            int y,
            Tile tile,
            int paletteOffset,
            bool flipX,
            bool flipY,
            ZeroIndexHandling zeroHandling)
        {
            for (int py = 0; py < tile.Height; py++)
            {
                for (int px = 0; px < tile.Width; px++)
                {
                    int sourceX = flipX ? (tile.Width - px - 1) : px;
                    int sourceY = flipY ? (tile.Height - py - 1) : py;
                    byte index = tile[sourceX, sourceY];

                    if (index > 0 || zeroHandling == ZeroIndexHandling.DrawSolid)
                        canvas.SetValue(x + px, y + py, index + paletteOffset);
                }
            }
        }

        internal static void AssignPalette(Bitmap image, Palette palette, ZeroIndexHandling zeroHandling)
        {
            var imagePalette = image.Palette;
            var imageColors = imagePalette.Entries;

            for (int i = 0; i < palette.TotalCount; i++)
            {
                if (i == 0 && zeroHandling != ZeroIndexHandling.DrawSolid)
                    imageColors[i] = Color.Transparent;
                else
                    imageColors[i] = palette.GetColor(i);
            }

            image.Palette = imagePalette;
        }
    }

    public enum ZeroIndexHandling
    {
        DrawSolid,
        DrawTransparent,
        DoNothing
    }
}
