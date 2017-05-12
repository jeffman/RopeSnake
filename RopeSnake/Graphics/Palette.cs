using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace RopeSnake.Graphics
{
    public class Palette : IEnumerable<Color>
    {
        public int Count => colors.Length;

        internal Color[] colors;
        public Color this[int index]
        {
            get => colors[index];
            set => colors[index] = value;
        }

        public Palette(int count)
        {
            colors = new Color[count];
        }

        public IEnumerator<Color> GetEnumerator()
            => ((IEnumerable<Color>)colors).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
