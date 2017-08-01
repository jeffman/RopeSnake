using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;
using RopeSnake.Mother3;

namespace RopeSnake.Tests.Mother3
{
    [TestClass]
    public class ConfigTests
    {
        private RomType origType = new RomType("Mother 3", "jp");

        [TestMethod]
        public void GetIntParameter()
        {
            Assert.AreEqual(256, Mother3Config.Configs[origType].GetParameter<int>("Items.Count"));
        }

        [TestMethod]
        public void GetStringParameter()
        {
            Assert.AreEqual("sar ", Mother3Config.Configs[origType].GetParameter<string>("Sar.Header"));
        }
    }
}
