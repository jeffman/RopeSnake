using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Text
{
    public sealed class ScriptEncodingParameters
    {
        [JsonProperty]
        public int EvenPadAddress { get; private set; }

        [JsonProperty]
        public int EvenPadModulus { get; private set; }

        [JsonProperty]
        public int EvenOffset1 { get; private set; }

        [JsonProperty]
        public int EvenOffset2 { get; private set; }

        [JsonProperty]
        public int OddPadAddress { get; private set; }

        [JsonProperty]
        public int OddPadModulus { get; private set; }

        [JsonProperty]
        public int OddOffset1 { get; private set; }

        [JsonProperty]
        public int OddOffset2 { get; private set; }
    }
}
