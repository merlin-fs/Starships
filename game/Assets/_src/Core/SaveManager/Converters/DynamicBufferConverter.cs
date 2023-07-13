using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Unity.Collections;
using Unity.Entities;

namespace Game.Core.Saves.Converters
{
    public class DynamicBufferConverter : JsonConverter
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var result = existingValue ?? Activator.CreateInstance(objectType);
            return result;
        }

        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}