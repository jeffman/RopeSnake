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

        public static T ReadJson<T>(this Stream stream)
        {
            var serializer = new JsonSerializer();
            using (var reader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        public static void WriteJson(this Stream stream, object value)
        {
            var serializer = new JsonSerializer();
            using (var writer = new StreamWriter(stream))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    serializer.Serialize(jsonWriter, value);
                }
            }
        }
    }
}
