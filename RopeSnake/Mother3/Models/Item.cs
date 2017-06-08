using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Mother3
{
    // http://datacrystal.romhacking.net/wiki/MOTHER_3:Item_data#Info_chunk
    public sealed class Item
    {
        internal const int FieldSize = 108;

        // The struct actually has an index into the item name table at offset 0,
        // but since it's more useful to edit the name directy we'll use a string
        // instead. Serializers need to take this into account.
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public bool Key { get; set; }
        public ushort SellPrice { get; set; }
        public EquipFlags EquipFlags { get; set; }
        public int Hp { get; set; }
        public short Pp { get; set; }
        public sbyte Offense { get; set; }
        public sbyte Defense { get; set; }
        public sbyte Iq { get; set; }
        public sbyte Speed { get; set; }
        public sbyte Kindness { get; set; }
        public Dictionary<AilmentType, short> AilmentProtection { get; set; }
        public Dictionary<ElementalType, sbyte> ElementalProtection { get; set; }
        public AttackType AttackType { get; set; }
        public BattleInfo BattleInfo { get; set; }
        public ushort UnknownA { get; set; }
        public bool SingleUse { get; set; }
        public byte UnknownB { get; set; }
    }

    public enum ItemType : int
    {
        Weapon = 0,
        Body = 1,
        Head = 2,
        Arms = 3,
        Food = 4,
        StatusHealer = 5,
        BattleA = 6,
        BattleB = 7,
        ImportantA = 8,
        ImportantB = 9
    }

    [Flags]
    public enum EquipFlags : ushort
    {
        None = 0x0000,
        EmptyA = 0x0001,
        Flint = 0x0002,
        Lucas = 0x0004,
        Duster = 0x0008,
        Kumatora = 0x0010,
        Boney = 0x0020,
        Salsa = 0x0040,
        Wess = 0x0080,
        Thomas = 0x0100,
        Ionia = 0x0200,
        Fuel = 0x0400,
        Alec = 0x0800,
        Fassad = 0x1000,
        Claus = 0x2000,
        EmptyB = 0x4000,
        EmptyC = 0x8000
    }

    public enum AttackType : byte
    {
        Bash = 0,
        Shoot = 1,
        Scratch = 2,
        Bite = 3,
        Kick = 4
    }
}
