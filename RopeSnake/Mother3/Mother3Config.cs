using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

                default:
                    RLog.Warn($"Unrecognized ROM version: {type.Version}. Defaulting to Japanese character map.");
                    goto case "jp";
            }
        }

        #endregion
    }
}
