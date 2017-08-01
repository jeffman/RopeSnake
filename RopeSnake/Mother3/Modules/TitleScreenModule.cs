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
    public sealed class TitleScreenModule : ModuleBase
    {
        public override string Name => "Mother3.TitleScreens";

        public override bool IsCompatibleWith(RomType romType)
            => romType.Game == "Mother 3";

        public override IEnumerable<Range> GetFreeRanges(RomType romType)
            => new Range[] { Range.StartEnd(0x1BD6940, 0x1C5F33F) };

        public override CompileResult Compile(ProjectData data, RomType romType)
        {
            var titleData = data as TitleScreenProjectData;
            var result = new CompileResult(romType, 4);

            var paletteBuffer = new Block(0x200);
            paletteBuffer.WritePalette(0, titleData.AnimationPalette);

            for (int i = 0; i < 21; i++)
            {
                var tileset = titleData.AnimationTilesets[i];
                var tilemap = titleData.AnimationTilemaps[i];

                var tilesetBuffer = new Block(tileset.Count * 64);
                var tilemapBuffer = new Block(0x800);
                
                tilesetBuffer.WriteTileset(0, titleData.AnimationTilesets[i], 8);
                tilemapBuffer.WriteTilemap(0, titleData.AnimationTilemaps[i]);

                result.AllocateBlocks.Add($"{Name}.AnimationTilesets[{i}]", tilesetBuffer);
                result.AllocateBlocks.Add($"{Name}.AnimationTilemaps[{i}]", tilemapBuffer);
            }

            result.AllocateBlocks.Add($"{Name}.AnimationPalette", paletteBuffer);

            return result;
        }

        public override ProjectData ReadFromProject(RomType romType, OpenResourceDelegate openResource)
        {
            var data = new TitleScreenProjectData();

            for (int i = 0; i < 21; i++)
            {
                string resource = $"TitleScreens\\Animation\\{i:D2}";
                RLog.Debug($"Reading {resource}.png");

                Bitmap bitmap;
                using (var bmpStream = openResource(resource, "png"))
                    bitmap = new Bitmap(bmpStream);

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
            var table = new OffsetTable(rom, rom.GetAsmPointer("TitleScreens"));
            var palette = rom.ReadPalette(table.GetEntry(51).Offset, 1, 256);

            data.AnimationPalette = palette;

            for (int i = 0; i < 21; i++)
            {
                var tileset = rom.ReadTileset(table.GetEntry(i + 9).Offset, 384, 8);
                var tilemap = rom.ReadTilemap(table.GetEntry(i + 30).Offset, 32, 32);
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
                using (var bmpStream = openResource(resource, "png"))
                    bitmap.Save(bmpStream, ImageFormat.Png);
            }
        }

        public override void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult)
        {
            CopyAllocatedBlocksToRom(rom, compileResult, allocationResult);

            var table = new OffsetTable(rom, rom.GetAsmPointer("TitleScreens"));

            for (int i = 0; i < 21; i++)
            {
                string tilesetKey = $"{Name}.AnimationTilesets[{i}]";
                string tilemapKey = $"{Name}.AnimationTilemaps[{i}]";

                int tilesetAddress = allocationResult.Allocations[tilesetKey];
                int tilemapAddress = allocationResult.Allocations[tilemapKey];

                table.UpdateEntry(i + 9, new TableEntry(tilesetAddress));
                table.UpdateEntry(i + 30, new TableEntry(tilemapAddress));
            }

            string paletteKey = $"{Name}.AnimationPalette";
            int paletteAddress = allocationResult.Allocations[paletteKey];
            table.UpdateEntry(51, paletteAddress);
        }
    }

    public class TitleScreenProjectData : ProjectData
    {
        public List<Tile>[] AnimationTilesets { get; set; } = new List<Tile>[21];
        public Tilemap[] AnimationTilemaps { get; set; } = new Tilemap[21];
        public Palette AnimationPalette { get; set; }
    }
}
