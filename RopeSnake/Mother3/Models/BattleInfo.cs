using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3
{
    // http://datacrystal.romhacking.net/wiki/MOTHER_3:Battle_tables_info_chunk
    public sealed class BattleInfo
    {
        internal const int FieldSize = 40;

        public int Effect { get; set; }
        public ElementalType Element { get; set; }
        public int Target { get; set; }
        public ushort Multiplier { get; set; }
        public ushort LowerStat { get; set; }
        public ushort UpperStat { get; set; }
        public AilmentType? Ailment { get; set; }
        public byte AilmentChance { get; set; }
        public AilmentMode AilmentMode { get; set; }
        public int Priority { get; set; }
        public ushort BattleText { get; set; }
        public bool AnimationDarken { get; set; }
        public byte Animation { get; set; }
        public byte HitAnimation { get; set; }
        public byte UnknownA { get; set; }
        public ushort Sound { get; set; }
        public byte MissChance { get; set; }
        public byte CriticalChance { get; set; }
        public bool Redirectable { get; set; }
        public byte UnknownB { get; set; }
    }

    public enum AilmentType : byte
    {
        Poison = 0,
        Paralysis = 1,
        Sleep = 2,
        Strange = 3,
        Cry = 4,
        Forgetful = 5,
        Nausea = 6,
        Fleas = 7,
        Burned = 8,
        Solidified = 9,
        Numb = 10,
        DCMC = 11
    }

    public enum ElementalType : int
    {
        Neutral = 0,
        Fire = 1,
        Freeze = 2,
        Thunder = 3,
        Bomb = 4
    }

    public enum AilmentMode : int
    {
        Heal = 0,
        Inflict = 1
    }
}
