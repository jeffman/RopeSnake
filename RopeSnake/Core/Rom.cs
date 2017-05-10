using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class Rom : AllocatableBlock
    {
        public RomType Type { get; private set; }

        public Rom() : base() { }

        public override void ReadFromFile(string fileName)
        {
            base.ReadFromFile(fileName);
            SetupRom();
        }

        protected void SetupRom()
        {
            Type = RomTypeDetector.Detect(this);

            var freeRanges = RomTypeDetector.GetFreeRanges(Type);
            if (freeRanges != null)
            {
                foreach (var range in freeRanges)
                    Deallocate(range);
            }
        }
    }
}
