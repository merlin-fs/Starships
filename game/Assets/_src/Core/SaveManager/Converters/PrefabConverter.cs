using System;
using Game.Core.Prefabs;
using Game.Model;
using Newtonsoft.Json;
using Unity.Entities;

namespace Game.Core.Saves.Converters
{
    public class PrefabConverter : DefaultConverter<PrefabRef>
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return null;//base.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            EntityManager manager = (EntityManager)serializer.Context.Context;
            var data = manager.GetComponentData<BakedPrefab>(((PrefabRef)value).Prefab);
            //writer.WritePropertyName(nameof(data.ConfigID));
            serializer.Serialize(writer, data.ConfigID.ToString());
        }
    }
}