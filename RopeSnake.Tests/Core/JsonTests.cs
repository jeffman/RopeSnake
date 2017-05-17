using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;
using Newtonsoft.Json;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class JsonTests
    {
        [TestMethod]
        public void AsmPointerRead()
        {
            string json = "{\"Location\":1234,\"TargetOffset\":56}";
            AsmPointer pointer = JsonConvert.DeserializeObject<AsmPointer>(json);

            Assert.AreEqual(1234, pointer.Location);
            Assert.AreEqual(56, pointer.TargetOffset);
        }

        [TestMethod]
        public void AsmPointerReadNoOffset()
        {
            string json = "1234";
            AsmPointer pointer = JsonConvert.DeserializeObject<AsmPointer>(json);

            Assert.AreEqual(1234, pointer.Location);
            Assert.AreEqual(0, pointer.TargetOffset);
        }

        [TestMethod]
        public void AsmPointerWrite()
        {
            string json = JsonConvert.SerializeObject(new AsmPointer(1234, 56));
            Assert.AreEqual("{\"Location\":1234,\"TargetOffset\":56}", json);
        }

        [TestMethod]
        public void AsmPointerWriteNoOffset()
        {
            string json = JsonConvert.SerializeObject(new AsmPointer(1234, 0));
            Assert.AreEqual("1234", json);
        }
    }
}
