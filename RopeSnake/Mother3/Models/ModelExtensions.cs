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
                UnknownA = stream.GetByte(),
                Sound = stream.ReadUShort(),
                MissChance = stream.GetByte(),
                CriticalChance = stream.GetByte(),
                Redirectable = stream.ReadBool8(),
                UnknownB = stream.GetByte()
            };
            return info;
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
            stream.WriteByte(info.UnknownA);
            stream.WriteUShort(info.Sound);
            stream.WriteByte(info.MissChance);
            stream.WriteByte(info.CriticalChance);
            stream.WriteBool8(info.Redirectable);
            stream.WriteByte(info.UnknownB);
        }
    }
}
