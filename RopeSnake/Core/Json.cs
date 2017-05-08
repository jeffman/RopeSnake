using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace RopeSnake.Core
{
    public static class Json
    {
        public static T ReadFromFile<T>(string path)
            => JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

        public static void WriteToFile(string path, object value)
            => File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented));
    }
}
