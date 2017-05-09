using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class ResourceManagerTests
    {
        private static ResourceManager manager;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            manager = new ResourceManager("temp");
        }

        private Stream Open(string name, string extension, FileMode mode)
            => manager.Get("test", name, extension, mode);

        private void Delete(string name, string extension)
            => manager.Delete("test", name, extension);

        [TestMethod]
        public void CreateNew()
        {
            string testString = "MrTenda";

            using (var stream = Open("new", "txt", FileMode.CreateNew))
            {
                stream.WriteString(testString);
            }

            var file = new FileInfo("temp\\new.txt");
            Assert.IsTrue(file.Exists);
            Assert.AreEqual(testString.Length, file.Length);
            Assert.AreEqual(testString, File.ReadAllText("temp\\new.txt"));
        }

        [TestMethod]
        public void CreateExisting()
        {
            File.WriteAllText("temp\\existing.txt", "To be overwritten");

            string testString = "MrTenda";

            using (var stream = Open("existing", "txt", FileMode.Create))
            {
                stream.WriteString(testString);
            }

            var file = new FileInfo("temp\\existing.txt");
            Assert.IsTrue(file.Exists);
            Assert.AreEqual(testString.Length, file.Length);
            Assert.AreEqual(testString, File.ReadAllText("temp\\existing.txt"));
        }

        [TestMethod]
        public void CreateWithNonExistingFolder()
        {
            string testString = "MrTenda";

            using (var stream = Open("foo\\existing", "txt", FileMode.Create))
            {
                stream.WriteString(testString);
            }

            var file = new FileInfo("temp\\foo\\existing.txt");
            Assert.IsTrue(file.Exists);
            Assert.AreEqual(testString.Length, file.Length);
            Assert.AreEqual(testString, File.ReadAllText("temp\\foo\\existing.txt"));
        }

        [TestMethod]
        public void DeleteExisting()
        {
            File.WriteAllText("temp\\existing.txt", "To be deleted");
            Delete("existing", "txt");
            Assert.IsFalse(File.Exists("temp\\existing.txt"));
        }
    }
}
