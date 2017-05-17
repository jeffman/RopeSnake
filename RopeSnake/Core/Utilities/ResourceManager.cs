using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileMapDict = System.Collections.Generic.Dictionary<string,
    System.Collections.Generic.Dictionary<string, string>>;

namespace RopeSnake.Core
{
    internal sealed class ResourceManager
    {
        public string Root { get; }
        public FileMapDict FileMap { get; set; }

        public ResourceManager(string root)
            : this(root, new FileMapDict())
        { }

        public ResourceManager(string root, FileMapDict fileMap)
        {
            Root = Path.GetFullPath(root);
            FileMap = fileMap ?? throw new ArgumentException(nameof(fileMap));
        }

        public void Delete(string module, string resource, string extension)
        {
            RLog.Trace($"Deleting resource [{module}][{resource}]");

            if (!FileMap.ContainsKey(module))
                throw new InvalidOperationException($"Module not found: {module}");

            if (!FileMap[module].ContainsKey(resource))
                throw new InvalidOperationException($"Resource not found [{module}]: {resource}");

            string fullPath = Path.Combine(Root, FileMap[module][resource]);
            var file = new FileInfo(fullPath);

            if (file.Exists)
            {
                RLog.Trace($"File exists; deleting {file.FullName}");
                file.Delete();
            }
            else
            {
                RLog.Trace($"File exists; skipping delete {file.FullName}");
            }

            FileMap[module].Remove(resource);
        }

        public Stream Get(string module, string resource, string extension, FileMode mode)
        {
            RLog.Trace($"Getting resource [{module}][{resource}] with mode {mode}");

            if (!FileMap.ContainsKey(module))
            {
                RLog.Trace($"Creating resource filemap for {module}");
                FileMap[module] = new Dictionary<string, string>();
            }

            if (!FileMap[module].ContainsKey(resource))
            {
                string defaultFile = $"{resource}.{extension}";
                RLog.Trace($"Creating default resource lookup for {module}: {defaultFile}");
                FileMap[module][resource] = defaultFile;
            }

            string fullPath = Path.Combine(Root, FileMap[module][resource]);
            var directory = new DirectoryInfo(Path.GetDirectoryName(fullPath));

            switch (mode)
            {
                case FileMode.Append:
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                    if (!directory.Exists)
                    {
                        RLog.Trace($"Creating directory {directory.FullName}");
                        directory.Create();
                    }
                    break;
            }

            RLog.Trace($"Opening resource {fullPath}");
            return File.Open(fullPath, mode);
        }
    }
}
