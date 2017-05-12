using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
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
            return new Range[] { Range.StartEnd(0x1BD4338, 0x1BD693F) };
        }

        public override CompileResult Compile(ProjectData data, RomType romType)
        {
            throw new NotImplementedException();
        }

        public override ProjectData ReadFromProject(RomType romType, OpenResourceDelegate openResource)
        {
            throw new NotImplementedException();
        }

        public override ProjectData ReadFromRom(Rom rom)
        {
            int gfxAddress = 0x1bd4338;
            int palAddress = 0x1bd5f40;
            int mapAddress = 0x1bd6140;

            var stream = rom.ToStream(gfxAddress);
            var tileset = stream.ReadCompressed(s => s.ReadTileset(192, 8));
            var palettes = stream.At(palAddress).ReadPalettes(1, 256);
            var tilemap = stream.At(mapAddress).ReadTilemap(32, 32);

            var bmp = new Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var canvas = new Canvas(bmp))
            {
                Render.Tilemap(canvas, tilemap, tileset, palettes);
            }

            var projectData = new TitleScreenProjectData { TitleScreen = bmp };
            return projectData;
        }

        public override void WriteToProject(ProjectData data, RomType romType, OpenResourceDelegate openResource)
        {
            var titleData = data as TitleScreenProjectData;
            titleData.TitleScreen.Save(openResource("TitleScreens\\Main", "png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        public override void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult)
        {
            throw new NotImplementedException();
        }
    }

    public class TitleScreenProjectData : ProjectData
    {
        public Bitmap TitleScreen { get; set; }
    }
}
