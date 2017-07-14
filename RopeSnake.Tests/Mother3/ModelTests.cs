using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Mother3;
using RopeSnake.Core;

namespace RopeSnake.Tests.Mother3
{
    [TestClass]
    public class ModelTests
    {
        static Rom origRom;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            origRom = new Rom();
            origRom.ReadFromFile("Artifacts\\Roms\\m3_orig.gba");
        }

        [TestMethod]
        public void ReadBattleInfo()
        {
            var stream = origRom.ToStream(0xE5108 + (0x5C * Item.SizeInBytes) + 0x40); // Favorite Food
            var oldPosition = stream.Position;
            var info = stream.ReadBattleInfo();

            Assert.AreEqual(BattleInfo.SizeInBytes, stream.Position - oldPosition);

            var expectedInfo = new BattleInfo
            {
                Effect = 3,
                Element = ElementalType.Neutral,
                Target = 1,
                Multiplier = 0,
                LowerStat = 0x12C,
                UpperStat = 0x12C,
                Ailment = null,
                AilmentChance = 0,
                AilmentMode = AilmentMode.Heal,
                Priority = 2,
                BattleText = 0x146,
                AnimationDarken = false,
                Animation = 0,
                HitAnimation = 0x1C,
                Unknown = 0,
                Sound = 0,
                MissChance = 0,
                CriticalChance = 0,
                Redirectable = true
            };

            TestHelpers.AssertPublicInstancePropertiesEqualDeep(expectedInfo, info);
        }

        [TestMethod]
        public void WriteBattleInfo()
        {
            var info = new BattleInfo
            {
                Effect = 1,
                Element = ElementalType.Fire,
                Target = 2,
                Multiplier = 3,
                LowerStat = 4,
                UpperStat = 5,
                Ailment = AilmentType.DCMC,
                AilmentChance = 6,
                AilmentMode = AilmentMode.Inflict,
                Priority = 7,
                BattleText = 8,
                AnimationDarken = true,
                Animation = 9,
                HitAnimation = 10,
                Unknown = 11,
                Sound = 12,
                MissChance = 13,
                CriticalChance = 14,
                Redirectable = false
            };

            var block = new Block(BattleInfo.SizeInBytes);
            var stream = block.ToStream();
            stream.WriteBattleInfo(info);

            Assert.AreEqual(BattleInfo.SizeInBytes, stream.Position);
            CollectionAssert.AreEqual(new byte[]
            {
                1, 0, 0, 0,
                1, 0, 0, 0,
                2, 0, 0, 0,
                3, 0,
                4, 0,
                5, 0,
                12,
                6,
                1, 0, 0, 0,
                7, 0, 0, 0,
                8, 0,
                1,
                9,
                10,
                11,
                12, 0,
                13,
                14,
                0,
                15
            }, block.Data);
        }
    }
}
