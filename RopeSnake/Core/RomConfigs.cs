using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public static class RomConfigs
    {
        public static IReadOnlyDictionary<RomType, string> FileNameLookup { get; }

        static RomConfigs()
        {
            var fileToRomType = Assets.Open("RomConfigs\\configs.json").ReadJson<Dictionary<string, RomType>>();
            fileToRomType = Assets.Open("RomConfigs\\configs.json").ReadJson<Dictionary<string, RomType>>();

            FileNameLookup = new ReadOnlyDictionary<RomType, string>(fileToRomType.ToDictionary(kv => kv.Value, kv => kv.Key));
        }
    }
}
