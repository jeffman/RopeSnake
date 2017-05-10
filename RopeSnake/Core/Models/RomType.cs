using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    public struct RomType : IEquatable<RomType>
    {
        public const string UnknownString = "Unknown";
        public static readonly RomType Unknown = new RomType(UnknownString, UnknownString);

        public readonly string Game;
        public readonly string Version;

        public RomType(string game, string version)
        {
            Game = game;
            Version = version;
        }

        public bool Equals(RomType other)
        {
            return (Game == other.Game) && (Version == other.Version);
        }

        public static bool operator ==(RomType first, RomType second)
            => first.Equals(second);

        public static bool operator !=(RomType first, RomType second)
            => !(first == second);

        public override bool Equals(object obj)
        {
            if (obj is RomType other)
                return this == other;
            return false;
        }

        public override int GetHashCode()
            => Game.GetHashCode() ^ Version.GetHashCode();

        public override string ToString()
            => $"{Game}, {Version}";
    }
}
