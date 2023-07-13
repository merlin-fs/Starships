using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.Core.Saves.Converters
{
    public class DefaultConverter<T> : JsonConverter
    {
        private Type m_Type = typeof(T);
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jsonObject = JProperty.Load(reader).Value;
            if (jsonObject is JArray array)
            {
                var result = new object[array.Count]; 
                for (var i = 0; i < array.Count; i++)
                {
                    result[i] = ReadValue(array[i], objectType, existingValue, serializer);
                }
                return result;
            }
            else
                return ReadValue(jsonObject, objectType, existingValue, serializer);
        }

        private object ReadValue(JToken jsonObject, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var result = existingValue ?? Activator.CreateInstance(objectType);
            var properties = GetFields(objectType);
            foreach (var property in properties)
            {
                JToken value = jsonObject[property.Name];
                if (value != null && value.Type != JTokenType.Null)
                {
                    property.SetValue(result, value.ToObject(property.FieldType, serializer));
                }
            }

            return result;
        }
        
        private IEnumerable<FieldInfo> GetFields(IReflect type)
        {
            return type?.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<NonSerializedAttribute>(true) == null);
        }

        public override bool CanConvert(Type objectType)
        {
            return !objectType.IsPrimitive && !objectType.IsArray && 
                   objectType.GetCustomAttribute<SavedAttribute>(true) != null && m_Type.IsAssignableFrom(objectType);
        }
        
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var type = value.GetType();

            writer.WriteStartObject();
            var properties = GetFields(type);
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    writer.WritePropertyName(property.Name);
                    serializer.Serialize(writer, property.GetValue(value));
                }
            }
            writer.WriteEndObject();
        }            
    }
}