using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;
using Newtonsoft.Json.Linq;

namespace RopeSnake.Tests.Core
{
    [TestClass]
    public class ProjectTests
    {
        [TestMethod]
        public void LoadProject()
        {
            var project = Project.Load("Artifacts\\TestProject");

            Assert.AreEqual("Halo 3", project.Type.Game);
            Assert.AreEqual("Christmas", project.Type.Version);
            CollectionAssert.AreEquivalent(new string[] { "Armor" }, project.SkipCompiling.ToList());
            Assert.AreEqual("potato", project.Get("Guns", "Handheld/Potato", mode: FileMode.Open).ReadAllText());
            Assert.AreEqual("rocket", project.Get("Guns", "Shoulder/Rocket", mode: FileMode.Open).ReadAllText());
            // Assert.IsFabulous(project.Get("Armor", "Reindeer");
        }

        [TestMethod]
        public void SaveProject()
        {
            var project = new Project
            {
                Type = new RomType("Mother 4", "NA"),
                SkipCompiling = new string[] { "Engine" },
                ResourceManager = new ResourceManager("temp\\TestProject")
            };

            using (var saturn = project.Get("Script", "MrSaturn", "txt"))
                saturn.WriteString("boing");

            project.Save("temp\\TestProject");

            Assert.IsTrue(File.Exists("temp\\TestProject\\project.json"));
            Assert.IsTrue(File.Exists("temp\\TestProject\\MrSaturn.txt"));
            Assert.AreEqual("boing", File.ReadAllText("temp\\TestProject\\MrSaturn.txt"));

            var readback = Json.ReadFromFile<JObject>("temp\\TestProject\\project.json");
            Assert.AreEqual("Mother 4", readback["Type"]["Game"].ToObject<string>());
            Assert.AreEqual("NA", readback["Type"]["Version"].ToObject<string>());
            CollectionAssert.AreEquivalent(new string[] { "Engine" }, readback["SkipCompiling"].ToObject<List<string>>());
            Assert.AreEqual("MrSaturn.txt", readback["Resources"]["Script"]["MrSaturn"].ToObject<string>());
        }
    }
}
