using System;
using Game.Model.Stats;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.Core.Saves.Converters
{
    public class ModifierConverter : DefaultConverter<Stat>
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var stat = (Stat)value;
            var obj = new JObject(
                new JProperty("$id", stat.StatID),
                new JProperty("value", stat.Value)
                );
            obj.WriteTo(writer);
        }            
    }
}