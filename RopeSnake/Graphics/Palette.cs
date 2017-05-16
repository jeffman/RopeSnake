using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace RopeSnake.Graphics
{
    public class Palette
    {
        public int PaletteCount { get; }
        public int ColorsPerPalette { get; }
        public int TotalCount => PaletteCount * ColorsPerPalette;

        internal protected Color[,] _colors;

        public Palette(int paletteCount, int colorsPerPalette)
        {
            _colors = new Color[paletteCount, colorsPerPalette];
            PaletteCount = paletteCount;
            ColorsPerPalette = colorsPerPalette;
        }

        public Color GetColor(int paletteIndex, int colorIndex)
            => _colors[paletteIndex, colorIndex];

        public Color GetColor(int index)
            => GetColor(index / ColorsPerPalette, index % ColorsPerPalette);

        public void SetColor(int paletteIndex, int colorIndex, Color color)
            => _colors[paletteIndex, colorIndex] = color;

        public void SetColor(int index, Color color)
            => SetColor(index / ColorsPerPalette, index % ColorsPerPalette, color);

        public bool PaletteEquals(Palette other)
        {
            if (other == null)
                return false;

            if (TotalCount != other.TotalCount)
                return false;

            for (int i = 0; i < TotalCount; i++)
            {
                if (GetColor(i) != other.GetColor(i))
                    return false;
            }

            return true;
        }
    }
}
