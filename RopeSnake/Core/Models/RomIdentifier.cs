using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    public sealed class RomIdentifier
    {
        [JsonProperty]
        public int Offset { get; private set; }

        [JsonProperty]
        public byte[] Data { get; private set; }
    }
}
