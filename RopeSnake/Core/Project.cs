using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FileMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;

namespace RopeSnake.Core
{
    public sealed class Project
    {
        internal const string DefaultFileName = "project.json";
        internal const string DefaultExt = "bin";
        internal const FileMode DefaultMode = FileMode.OpenOrCreate;
        internal ResourceManager ResourceManager { get; set; }

        public List<string> SkipCompiling { get; internal set; }
        public RomType Type { get; internal set; }

        internal Project() { }

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

        public static Project Load(string projectDirectory)
        {
            projectDirectory = Path.GetFullPath(projectDirectory);
            var json = Json.ReadFromFile<JObject>(Path.Combine(projectDirectory, DefaultFileName));

            var resources = new ResourceManager(projectDirectory, json["Resources"].ToObject<FileMap>());
            var type = json["Type"].ToObject<RomType>();
            var skip = json["SkipCompiling"]?.ToObject<List<string>>() ?? new List<string>();

            return new Project
            {
                ResourceManager = resources,
                Type = type,
                SkipCompiling = skip
            };
        }

        public void Save(string projectDirectory)
        {
            projectDirectory = Path.GetFullPath(projectDirectory);
            var json = new JObject();

            json.Add("Type", JToken.FromObject(Type));
            json.Add("SkipCompiling", JToken.FromObject(SkipCompiling));
            json.Add("Resources", JToken.FromObject(ResourceManager.FileMap));

            Json.WriteToFile(Path.Combine(projectDirectory, DefaultFileName), json);
        }

        public void Delete(string module, string resource, string extension = DefaultExt)
            => ResourceManager.Delete(module, resource, extension);

        public Stream Get(string module, string resource, string extension = DefaultExt, FileMode mode = DefaultMode)
            => ResourceManager.Get(module, resource, extension, mode);
    }
}
