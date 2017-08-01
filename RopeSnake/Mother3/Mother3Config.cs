using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;
using RopeSnake.Mother3.Text;
using Newtonsoft.Json;

namespace RopeSnake.Mother3
{
    public sealed class Mother3Config
    {
        [JsonProperty]
        public Dictionary<string, AsmPointer[]> AsmPointers { get; internal set; }

        [JsonProperty]
        public ControlCode[] ControlCodes { get; internal set; }

        [JsonProperty]
        public Dictionary<short, char> NormalLookup { get; internal set; }

        [JsonProperty]
        public Dictionary<short, char> SaturnLookup { get; internal set; }

        [JsonProperty]
        public ScriptEncodingParameters ScriptEncodingParameters { get; internal set; }

        [JsonProperty]
        public Dictionary<string, object> Parameters { get; internal set; }

        [JsonProperty]
        public Dictionary<string, Range[]> FreeRanges { get; internal set; }

        #region Static members

        public static IReadOnlyDictionary<RomType, Mother3Config> Configs { get; internal set; }

        static Mother3Config()
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

        public static ICharacterMap CreateCharacterMap(RomType type)
        {
            switch (type.Version)
            {
                case "jp":
                    return new JapaneseCharacterMap(Configs[type].NormalLookup, Configs[type].SaturnLookup);

                case "en-v10":
                    return new EnglishCharacterMap(Configs[type].NormalLookup, Configs[type].SaturnLookup);

                default:
                    RLog.Warn($"Unrecognized ROM version: {type.Version}. Defaulting to Japanese character map.");
                    goto case "jp";
            }
        }

        #endregion

        public T GetParameter<T>(string key)
        {
            return (T)Convert.ChangeType(Parameters[key], typeof(T));
        }
    }
}
