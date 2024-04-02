using System;
using Game.Core.Prefabs;
using Newtonsoft.Json;

namespace Game.Core.Saves.Converters
{
#nullable enable        
    public class PrefabConverter : DefaultConverter<PrefabInfo>
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return null;//base.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            
            var info = (PrefabInfo)(value ?? default(PrefabInfo));
            serializer.Serialize(writer, info.ConfigID.ToString());
        }
    }
#nullable disable        
}