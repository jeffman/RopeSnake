using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using RopeSnake.Core;
using RopeSnake.Gba;
using RopeSnake.Graphics;

namespace RopeSnake.Mother3
{
    public class TitleScreenModule : ModuleBase
    {
        public override string Name => "Mother3.TitleScreens";

        public override bool IsCompatibleWith(RomType romType)
        {
            return romType.Game == "Mother 3";
        }

        public override IEnumerable<Range> GetFreeRanges(RomType romType)
        {
            return new Range[] { Range.StartEnd(0x1BD6940, 0x1C5F33F) };
        }

        public override CompileResult Compile(ProjectData data, RomType romType)
        {
            throw new NotImplementedException();
        }

        public override ProjectData ReadFromProject(RomType romType, OpenResourceDelegate openResource)
        {
            var data = new TitleScreenProjectData();

            for (int i = 0; i < 21; i++)
            {
                string resource = $"TitleScreens\\Animation\\{i:D2}";
                RLog.Debug($"Reading {resource}.png");

                var bitmap = new Bitmap(openResource(resource, "png"));
                var graphics = GbaGraphicsUtilities.ParseBitmap(bitmap, 1, 256);

                data.AnimationTilesets[i] = graphics.Tileset;
                data.AnimationTilemaps[i] = graphics.Tilemap;

                if (i == 0)
                    data.AnimationPalette = graphics.Palette;
                else
                {
                    if (!graphics.Palette.PaletteEquals(data.AnimationPalette))
                        RLog.Warn($"Title screen animation {resource}.png has differing palette from 00.png");
                }
            }

            return data;
        }

        public override ProjectData ReadFromRom(Rom rom)
        {
            var data = new TitleScreenProjectData();
            var table = new OffsetTableAccessor(rom, 0x1BCDD8C);
            var palette = table.ParseEntry(51, rom, s => s.ReadPalette(1, 256));

            data.AnimationPalette = palette;

            for (int i = 0; i < 21; i++)
            {
                var tileset = table.ParseEntry(i + 9, rom, s => s.ReadTileset(384, 8));
                var tilemap = table.ParseEntry(i + 30, rom, s => s.ReadTilemap(32, 32));
                data.AnimationTilesets[i] = tileset;
                data.AnimationTilemaps[i] = tilemap;
            }

            return data;
        }

        public override void WriteToProject(ProjectData data, RomType romType, OpenResourceDelegate openResource)
        {
            var titleData = data as TitleScreenProjectData;

            for (int i = 0; i < 21; i++)
            {
                var tileset = titleData.AnimationTilesets[i];
                var tilemap = titleData.AnimationTilemaps[i];

                var bitmap = new Bitmap(256, 256, PixelFormat.Format8bppIndexed);

                using (var canvas = Canvas.Create(bitmap))
                {
                    Render.Tilemap(canvas, tilemap, tileset, titleData.AnimationPalette, ZeroIndexHandling.DrawSolid);
                }

                string resource = $"TitleScreens\\Animation\\{i:D2}";
                bitmap.Save(openResource(resource, "png"), ImageFormat.Png);
            }
        }

        public override void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult)
        {
            throw new NotImplementedException();
        }
    }

    public class TitleScreenProjectData : ProjectData
    {
        public List<Tile>[] AnimationTilesets { get; set; } = new List<Tile>[21];
        public Tilemap[] AnimationTilemaps { get; set; } = new Tilemap[21];
        public Palette AnimationPalette { get; set; }
    }
}
