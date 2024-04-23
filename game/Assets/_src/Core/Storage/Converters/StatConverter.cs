using System;
using Game.Model.Stats;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.Core.Storages.Converters
{
#nullable enable        
    public class StatConverter : DefaultConverter<Stat>
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var stat = (Stat)(value ?? default(Stat));
            var obj = new JObject(
                new JProperty("$id", stat.ToString()),
                new JProperty("value", stat.Value)
                );
            obj.WriteTo(writer);
        }            
    }
#nullable disable        
}