using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RopeSnake.Core
{
    [JsonConverter(typeof(AsmPointerJsonConverter))]
    public struct AsmPointer
    {
        public int Location { get; private set; }
        public int TargetOffset { get; private set; }

        public AsmPointer(int location, int targetOffset)
        {
            Location = location;
            TargetOffset = targetOffset;
        }
    }

    internal sealed class AsmPointerJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(AsmPointer);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                int location = serializer.Deserialize<int>(reader);
                return new AsmPointer(location, 0);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // James Newton-King, if you're reading this, we would all really
                // appreciate a way to fall back to default serialization...
                var obj = JToken.ReadFrom(reader);
                return new AsmPointer((int)obj["Location"], (int)obj["TargetOffset"]);
            }

            throw new JsonException($"Unexpected token when reading AsmPointer: {reader.Path}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is AsmPointer pointer)
            {
                if (pointer.TargetOffset == 0)
                    serializer.Serialize(writer, pointer.Location);
                else
                {
                    // I'll buy you beer if you do it
                    var obj = JObject.FromObject(new
                    {
                        Location = pointer.Location,
                        TargetOffset = pointer.TargetOffset
                    });
                    serializer.Serialize(writer, obj);
                }
            }
            else
            {
                throw new JsonException($"Unexpected object type when writing AsmPointer: {value.GetType()}");
            }
        }
    }
}
