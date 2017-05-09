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

        private Project() { }

        public static Project CreateNew(string path)
        {
            var project = new Project
            {
                ResourceManager = new ResourceManager(path)
            };

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

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
