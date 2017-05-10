using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public sealed class Project
    {
        internal const string DefaultFileName = "project.json";
        internal const string DefaultExt = "bin";
        internal const FileMode DefaultMode = FileMode.OpenOrCreate;
        internal ResourceManager ResourceManager { get; set; }

        public HashSet<string> SkipCompiling { get; set; }
        public RomType Type { get; internal set; }

        private Project() { }

        public static Project CreateNew(string projectDirectory, RomType type)
        {
            var project = new Project
            {
                Type = type,
                ResourceManager = new ResourceManager(projectDirectory)
            };

            if (!Directory.Exists(projectDirectory))
                Directory.CreateDirectory(projectDirectory);

            return project;
        }

        public static Project Load(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Delete(string module, string resource, string extension = DefaultExt)
            => ResourceManager.Delete(module, resource, extension);

        public Stream Get(string module, string resource, string extension, FileMode mode = DefaultMode)
            => ResourceManager.Get(module, resource, extension, mode);
    }
}
