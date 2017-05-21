using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Mother3.Text
{
    public sealed class ControlCode
    {
        [JsonProperty]
        public string Description { get; private set; }

        [JsonProperty]
        public short Value { get; private set; }

        [JsonProperty]
        public int Arguments { get; private set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Tag { get; private set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ControlCodeFlags Flags { get; private set; }
    }

    [Flags]
    public enum ControlCodeFlags
    {
        None = 0,
        Terminate = 1,
        AlternateFont = 2
    }
}
