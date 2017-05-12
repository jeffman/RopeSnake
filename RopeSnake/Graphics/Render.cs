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
            _zeroHandlers[ZeroIndexHandling.DrawSolid] = (canvas, x, y, color) => { canvas.SetPixel(x, y, color); };
            _zeroHandlers[ZeroIndexHandling.DrawTransparent] = (canvas, x, y, color) => { canvas.SetPixel(x, y, Color.Transparent); };
        }

        public static void Tilemap(
            Canvas canvas,
            Tilemap tilemap,
            IEnumerable<Tile> tileset,
            IEnumerable<Palette> palettes,
            ZeroIndexHandling zeroHandling = ZeroIndexHandling.DoNothing,
            Region clippingRegion = null)
        {
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

                    if (clippingRegion == null || !clippingRegion.IsVisible(tileRect))
                    {
                        Render.TilePixels(canvas,
                            tileRect.X,
                            tileRect.Y,
                            tile,
                            palettes.ElementAt(info.Palette),
                            info.FlipX,
                            info.FlipY,
                            zeroHandling);
                    }
                }
            }
        }

        public static void TilePixels(
            Canvas canvas,
            int x,
            int y,
            Tile tile,
            Palette palette,
            bool flipX,
            bool flipY,
            ZeroIndexHandling zeroHandling)
        {
            for (int py = 0; py < tile.Height; py++)
            {
                for (int px = 0; px < tile.Width; px++)
                {
                    int sourceX = flipX ? (tile.Width - px) : px;
                    int sourceY = flipY ? (tile.Height - py) : py;
                    byte index = tile[sourceX, sourceY];

                    if (index == 0)
                        _zeroHandlers[zeroHandling](canvas, x + px, y + py, palette[index]);
                    else
                        canvas.SetPixel(x + px, y + py, palette[index]);
                }
            }
        }
    }

    public enum ZeroIndexHandling
    {
        DrawSolid,
        DrawTransparent,
        DoNothing
    }
}
