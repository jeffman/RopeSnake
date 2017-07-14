using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public static class ModelExtensions
    {
        public static BattleInfo ReadBattleInfo(this Block block, int offset)
            => new BattleInfo
            {
                Effect = block.ReadInt(offset),
                Element = (ElementalType)block.ReadInt(offset + 4),
                Target = block.ReadInt(offset + 8),
                Multiplier = block.ReadUShort(offset + 12),
                LowerStat = block.ReadUShort(offset + 14),
                UpperStat = block.ReadUShort(offset + 16),
                Ailment = DecodeAilmentType(block.ReadByte(offset + 18)),
                AilmentChance = block.ReadByte(offset + 19),
                AilmentMode = (AilmentMode)block.ReadInt(offset + 20),
                Priority = block.ReadInt(offset + 24),
                BattleText = block.ReadUShort(offset + 28),
                AnimationDarken = block.ReadBool8(offset + 30),
                Animation = block.ReadByte(offset + 31),
                HitAnimation = block.ReadByte(offset + 32),
                Unknown = block.ReadByte(offset + 33),
                Sound = block.ReadUShort(offset + 34),
                MissChance = block.ReadByte(offset + 36),
                CriticalChance = block.ReadByte(offset + 37),
                Redirectable = block.ReadBool16(offset + 38)
            };

        public static void WriteBattleInfo(this Block block, int offset, BattleInfo info)
        {
            block.WriteInt(offset, info.Effect);
            block.WriteInt(offset + 4, (int)info.Element);
            block.WriteInt(offset + 8, info.Target);
            block.WriteUShort(offset + 12, info.Multiplier);
            block.WriteUShort(offset + 14, info.LowerStat);
            block.WriteUShort(offset + 16, info.UpperStat);
            block.WriteByte(offset + 18, EncodeAilmentType(info.Ailment));
            block.WriteByte(offset + 19, info.AilmentChance);
            block.WriteInt(offset + 20, (int)info.AilmentMode);
            block.WriteInt(offset + 24, info.Priority);
            block.WriteUShort(offset + 28, info.BattleText);
            block.WriteBool8(offset + 30, info.AnimationDarken);
            block.WriteByte(offset + 31, info.Animation);
            block.WriteByte(offset + 32, info.HitAnimation);
            block.WriteByte(offset + 33, info.Unknown);
            block.WriteUShort(offset + 34, info.Sound);
            block.WriteByte(offset + 36, info.MissChance);
            block.WriteByte(offset + 37, info.CriticalChance);
            block.WriteBool16(offset + 38, info.Redirectable);
        }

        internal static AilmentType? DecodeAilmentType(byte value)
        {
            if (value == 0)
                return null;
            else
                return (AilmentType)(value - 1);
        }

        internal static byte EncodeAilmentType(AilmentType? value)
        {
            if (value == null)
                return 0;
            else
                return (byte)((byte)value.Value + 1);
        }

        public static Item ReadItem(this Block block, int offset)
        {
            var item = new Item
            {
                Index = block.ReadInt(offset),
                Type = (ItemType)block.ReadInt(offset + 4),
                Key = !block.ReadBool16(offset + 8),
                SellPrice = block.ReadUShort(offset + 10),
                EquipFlags = (EquipFlags)block.ReadInt(offset + 12),
                Hp = block.ReadInt(offset + 16),
                Pp = block.ReadInt(offset + 20),
                Offense = block.ReadSByte(offset + 24),
                Defense = block.ReadSByte(offset + 25),
                Iq = block.ReadSByte(offset + 26),
                Speed = block.ReadSByte(offset + 27),
                Kindness = block.ReadInt(offset + 28),
                AilmentProtection = Enumerable.Range(0, 11).ToDictionary(i => (AilmentType)i, i => block.ReadShort(offset + 32 + i * 2)),
                ElementalProtection = Enumerable.Range(0, 5).ToDictionary(i => (ElementalType)i, i => block.ReadSByte(offset + 54 + i)),
                AttackType = (AttackType)block.ReadByte(offset + 59),
                UnknownA = block.ReadInt(offset + 60),
                BattleInfo = block.ReadBattleInfo(offset + 64),
                UnknownB = block.ReadUShort(offset + 104),
                SingleUse = block.ReadBool16(offset + 106)
            };

            return item;
        }

        public static void WriteItem(this Block block, int offset, Item item)
        {
            block.WriteInt(offset, item.Index);
            block.WriteInt(offset + 4, (int)item.Type);
            block.WriteBool16(offset + 8, !item.Key);
            block.WriteUShort(offset + 10, item.SellPrice);
            block.WriteInt(offset + 12, (int)item.EquipFlags);
            block.WriteInt(offset + 16, item.Hp);
            block.WriteInt(offset + 20, item.Pp);
            block.WriteSByte(offset + 24, item.Offense);
            block.WriteSByte(offset + 25, item.Defense);
            block.WriteSByte(offset + 26, item.Iq);
            block.WriteSByte(offset + 27, item.Speed);
            block.WriteInt(offset + 28, item.Kindness);

            for (int i = 0; i < 11; i++)
                block.WriteShort(offset + 32 + i * 2, item.AilmentProtection[(AilmentType)i]);

            for (int i = 0; i < 5; i++)
                block.WriteSByte(offset + 54 + i, item.ElementalProtection[(ElementalType)i]);

            block.WriteByte(offset + 59, (byte)item.AttackType);
            block.WriteInt(offset + 60, item.UnknownA);
            block.WriteBattleInfo(offset + 64, item.BattleInfo);
            block.WriteUShort(offset + 104, item.UnknownB);
            block.WriteBool16(offset + 106, item.SingleUse);
        }
    }
}
