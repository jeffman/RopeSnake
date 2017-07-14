using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3
{
    public static class ModelExtensions
    {
        public static BattleInfo ReadBattleInfo(this Stream stream)
        {
            var info = new BattleInfo
            {
                Effect = stream.ReadInt(),
                Element = (ElementalType)stream.ReadInt(),
                Target = stream.ReadInt(),
                Multiplier = stream.ReadUShort(),
                LowerStat = stream.ReadUShort(),
                UpperStat = stream.ReadUShort(),
                Ailment = DecodeAilmentType(stream.GetByte()),
                AilmentChance = stream.GetByte(),
                AilmentMode = (AilmentMode)stream.ReadInt(),
                Priority = stream.ReadInt(),
                BattleText = stream.ReadUShort(),
                AnimationDarken = stream.ReadBool8(),
                Animation = stream.GetByte(),
                HitAnimation = stream.GetByte(),
                Unknown = stream.GetByte(),
                Sound = stream.ReadUShort(),
                MissChance = stream.GetByte(),
                CriticalChance = stream.GetByte(),
                Redirectable = stream.ReadBool16()
            };
            return info;
        }

        public static void WriteBattleInfo(this Stream stream, BattleInfo info)
        {
            stream.WriteInt(info.Effect);
            stream.WriteInt((int)info.Element);
            stream.WriteInt(info.Target);
            stream.WriteUShort(info.Multiplier);
            stream.WriteUShort(info.LowerStat);
            stream.WriteUShort(info.UpperStat);
            stream.WriteByte(EncodeAilmentType(info.Ailment));
            stream.WriteByte(info.AilmentChance);
            stream.WriteInt((int)info.AilmentMode);
            stream.WriteInt(info.Priority);
            stream.WriteUShort(info.BattleText);
            stream.WriteBool8(info.AnimationDarken);
            stream.WriteByte(info.Animation);
            stream.WriteByte(info.HitAnimation);
            stream.WriteByte(info.Unknown);
            stream.WriteUShort(info.Sound);
            stream.WriteByte(info.MissChance);
            stream.WriteByte(info.CriticalChance);
            stream.WriteBool16(info.Redirectable);
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

        public static Item ReadItem(this Stream stream)
        {
            var item = new Item
            {
                Index = stream.ReadInt(),
                Type = (ItemType)stream.ReadInt(),
                Key = !stream.ReadBool16(),
                SellPrice = stream.ReadUShort(),
                EquipFlags = (EquipFlags)stream.ReadInt(),
                Hp = stream.ReadInt(),
                Pp = stream.ReadInt(),
                Offense = stream.ReadSByte(),
                Defense = stream.ReadSByte(),
                Iq = stream.ReadSByte(),
                Speed = stream.ReadSByte(),
                Kindness = stream.ReadInt(),
                AilmentProtection = Enumerable.Range(0, 11).ToDictionary(i => (AilmentType)i, i => stream.ReadShort()),
                ElementalProtection = Enumerable.Range(0, 5).ToDictionary(i => (ElementalType)i, i => stream.ReadSByte()),
                AttackType = (AttackType)stream.GetByte(),
                UnknownA = stream.ReadInt(),
                BattleInfo = stream.ReadBattleInfo(),
                UnknownB = stream.ReadUShort(),
                SingleUse = stream.ReadBool16()
            };

            return item;
        }

        public static void WriteItem(this Stream stream, Item item)
        {
            stream.WriteInt(item.Index);
            stream.WriteInt((int)item.Type);
            stream.WriteBool16(!item.Key);
            stream.WriteUShort(item.SellPrice);
            stream.WriteInt((int)item.EquipFlags);
            stream.WriteInt(item.Hp);
            stream.WriteInt(item.Pp);
            stream.WriteSByte(item.Offense);
            stream.WriteSByte(item.Defense);
            stream.WriteSByte(item.Iq);
            stream.WriteSByte(item.Speed);
            stream.WriteInt(item.Kindness);

            for (int i = 0; i < 11; i++)
                stream.WriteShort(item.AilmentProtection[(AilmentType)i]);

            for (int i = 0; i < 5; i++)
                stream.WriteSByte(item.ElementalProtection[(ElementalType)i]);

            stream.WriteByte((byte)item.AttackType);
            stream.WriteInt(item.UnknownA);
            stream.WriteBattleInfo(item.BattleInfo);
            stream.WriteUShort(item.UnknownB);
            stream.WriteBool16(item.SingleUse);
        }
    }
}
