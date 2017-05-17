using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public static class Mother3
    {
        public static IReadOnlyDictionary<RomType, Mother3Config> Configs { get; internal set; }

        static Mother3()
        {
            ReloadConfigs();
        }

        public static void ReloadConfigs()
        {
            var configs = new Dictionary<RomType, Mother3Config>();

            foreach (var kv in RomConfigs.FileNameLookup.Where(kv => kv.Key.Game == "Mother 3"))
                configs.Add(kv.Key, Assets.Open($"RomConfigs\\{kv.Value}.json").ReadJson<Mother3Config>());

            Configs = new ReadOnlyDictionary<RomType, Mother3Config>(configs);
        }
    }
}
