using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;
using Newtonsoft.Json;

namespace RopeSnake.Mother3
{
    public sealed class Mother3Config
    {
        [JsonProperty]
        public Dictionary<string, AsmPointer[]> AsmPointers { get; internal set; }

        public int GetAsmPointer(string key, Rom rom)
        {
            var pointers = new HashSet<int>();

            foreach (var asmPointer in AsmPointers[key])
            {
                int pointer = rom.ReadInt(asmPointer.Location).FromPointer();
                if (pointer > 0)
                    pointer += asmPointer.TargetOffset;

                pointers.Add(pointer);
            }

            if (pointers.Count > 1)
                throw new Exception($"Differing ASM pointers found for {key}: {String.Join(", ", pointers.Select(p => $"0x{p:X}"))}");

            return pointers.First();
        }
    }
}
