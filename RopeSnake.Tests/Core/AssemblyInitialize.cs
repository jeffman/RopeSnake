using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class AssemblyInitialize
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Directory.CreateDirectory("temp");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Directory.Delete("temp", true);
        }

        [TestMethod]
        public void TempFolderExists()
        {
            Assert.IsTrue(Directory.Exists("temp"));
        }
    }
}
