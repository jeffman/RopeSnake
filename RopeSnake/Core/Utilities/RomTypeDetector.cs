using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    public static class RomTypeDetector
    {
        private class RomTypeInfo
        {
            public string Platform { get; set; }
            public List<RomIdentifier> Identifiers { get; set; }
            public Dictionary<string, RomVersionInfo> Versions { get; set; }
        }

        private class RomVersionInfo
        {
            public List<RomIdentifier> Identifiers { get; set; }
            public List<Range> FreeRanges { get; set; }
        }

        private static Dictionary<string, RomTypeInfo> _info;

        static RomTypeDetector()
        {
            _info = Assets.Open("romtypes.json").ReadJson<Dictionary<string, RomTypeInfo>>();
        }

        public static RomType Detect(Block block)
        {
            if (block == null || block.Data == null)
                throw new ArgumentNullException(nameof(block));

            var matchingGame = FindMatch(block, _info, kv => kv.Value.Identifiers, "game");
            if (matchingGame == null)
                return RomType.Unknown;

            var game = matchingGame.Value;

            var matchingVersion = FindMatch(block, game.Value.Versions, kv => kv.Value.Identifiers, "version");
            if (matchingVersion == null)
                return new RomType(game.Key, RomType.UnknownString);

            return new RomType(game.Key, matchingVersion.Value.Key);
        }

        private static KeyValuePair<string, T>? FindMatch<T>(
            Block block,
            IEnumerable<KeyValuePair<string, T>> toSearch,
            Func<KeyValuePair<string, T>, IEnumerable<RomIdentifier>> identifierSelector,
            string kind)
        {
            var matches = toSearch.Where(kv => HasIdentifiers(block, identifierSelector(kv))).ToArray();

            if (matches.Length > 1)
                throw new Exception($"Found more than 1 matching {kind}: {string.Join(", ", matches.Select(g => g.Key))}");

            if (matches.Length == 0)
            {
                RLog.Debug($"Could not find a matching {kind}. Returning null");
                return null;
            }

            return matches.First();
        }

        private static bool HasIdentifier(Block block, RomIdentifier identifier)
        {
            if (identifier.Offset + identifier.Data.Length > block.Length)
                return false;

            return block.Data
                .Skip(identifier.Offset)
                .Take(identifier.Data.Length)
                .SequenceEqual(identifier.Data);
        }

        private static bool HasIdentifiers(Block block, IEnumerable<RomIdentifier> identifiers)
            => identifiers.All(r => HasIdentifier(block, r));

        public static IEnumerable<Range> GetFreeRanges(RomType type)
        {
            if (_info.TryGetValue(type.Game, out var typeInfo))
            {
                if (typeInfo.Versions.TryGetValue(type.Version, out var versionInfo))
                    return versionInfo.FreeRanges.ToArray();
            }

            return null;
        }
    }
}
