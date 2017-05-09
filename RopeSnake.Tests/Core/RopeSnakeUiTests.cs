using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Core;

namespace RopeSnake.Tests.Core
{
    public class TestModule : IModule
    {
        public string Name => "TestModule";

#pragma warning disable CS0067
        public event ModuleProgressEventHandler Progress;
#pragma warning restore CS0067

        public virtual void Reset()
            => throw new NotImplementedException();

        public virtual ProjectData ReadFromProject(OpenResourceDelegate openResource)
            => throw new NotImplementedException();

        public virtual ProjectData ReadFromRom(Rom rom)
            => throw new NotImplementedException();

        public virtual void WriteToProject(ProjectData data, OpenResourceDelegate openResource)
            => throw new NotImplementedException();

        public virtual void WriteToRom(Rom rom, CompileResult compileResult, AllocationResult allocationResult)
            => throw new NotImplementedException();

        public virtual CompileResult Compile(ProjectData data)
            => throw new NotImplementedException();
    }

    [TestClass]
    public class RopeSnakeUiTests
    {
        [TestMethod]
        public void LoadModule()
        {
            var module = RopeSnakeUi.LoadModule(typeof(TestModule));

            Assert.IsNotNull(module);
            Assert.IsInstanceOfType(module, typeof(IModule));
            Assert.AreEqual("TestModule", module.Name);
        }

        [TestMethod]
        public void FindModuleTypes()
        {
            // Add more module types here as they are defined
            var expected = new Type[]
            {
                typeof(TestModule)
            };

            CollectionAssert.AreEquivalent(expected, RopeSnakeUi.ModuleTypes.ToList());
        }
    }
}
