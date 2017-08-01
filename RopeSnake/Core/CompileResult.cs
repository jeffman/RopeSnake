using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public sealed class CompileResult
    {
        /// <summary>
        /// Blocks to be compiled to fixed ROM locations.
        /// </summary>
        public Dictionary<string, (Block, int)> StaticBlocks { get; }

        /// <summary>
        /// Blocks to be allocated to free ROM space.
        /// </summary>
        public Dictionary<string, Block> AllocateBlocks { get; }

        /// <summary>
        /// ROM type associated with this compilation.
        /// </summary>
        public RomType RomType { get; }

        /// <summary>
        /// Alignment to be used during allocation.
        /// </summary>
        public int Alignment { get; }

        /// <summary>
        /// Optional data to be included for use during WriteToRom.
        /// </summary>
        public object Tag { get; set; }

        public CompileResult(RomType type, int alignment = 1)
        {
            RomType = type;
            Alignment = alignment;
            StaticBlocks = new Dictionary<string, (Block, int)>();
            AllocateBlocks = new Dictionary<string, Block>();
        }
    }
}
