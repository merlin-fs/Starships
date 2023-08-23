using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using Game.Model;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Core.Saves.Converters
{
#nullable enable        
    public class EntityConverter : DefaultConverter<Entity>
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var result = existingValue ?? Activator.CreateInstance(objectType);
            var properties = GetFields(objectType);
            //.Where(p => !p.GetCustomAttributes(false).Any(a => _ignoredAttributes.Contains(a.GetType())));
            foreach (var property in properties)
            {
                JToken? value = jsonObject[property.Name];
                if (value != null && value.Type != JTokenType.Null)
                {
                    property.SetValue(result, value.ToObject(property.FieldType, serializer));
                }
            }
            return result;
        }

        private IEnumerable<FieldInfo> GetFields(IReflect type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }
        
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) return;
            
            EntityManager manager = (EntityManager)serializer.Context.Context;
            Entity entity = (Entity)value;
            if (entity == Entity.Null)
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
                return;
            };
            serializer.ReferenceResolver?.GetReference(serializer, value);
            
            var savedType = typeof(SavedAttribute);
            var components = manager.GetComponentTypes(entity)
                .Where(cmp => cmp.GetManagedType().GetCustomAttributes(true).Any(a => a.GetType() == savedType));

            writer.WriteStartObject();
            writer.WritePropertyName("$id");
            writer.WriteValue(entity.Index);
            foreach (var iter in components)
            {
                var type = iter.GetManagedType();
                var data = GetComponentData(manager, entity, iter);
                if (type == typeof(Move))
                {
                    LocalTransform transform = (LocalTransform)GetComponentData(manager, entity, ComponentType.ReadOnly<LocalTransform>());
                    var move = (Move)data;
                    move.Position = transform.Position;
                    move.Rotation = transform.Rotation;
                    data = move;
                }
                writer.WritePropertyName(type.FullName);
                serializer.Serialize(writer, data, type);
            }
            writer.WriteEndObject();
        }

        public static object GetComponentData(EntityManager manager, Entity entity, ComponentType typeComponent)
        {
            if (typeComponent.IsBuffer)
            {
                //public DynamicBuffer<T> GetBuffer<T>(Entity entity, bool isReadOnly = false) where T : unmanaged, IBufferElementData
                var method = manager.GetType().GetMethods()
                    .First(m => m.Name == "GetBuffer" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(Entity));
                var getData = method.MakeGenericMethod(typeComponent.GetManagedType());
                var buff = getData.Invoke(manager, new object[] { entity, true });
                return buff.GetType().GetMethod("AsNativeArray").Invoke(buff, new object[] {});
            }
            else
            {
                //GetComponentData<T>(Entity entity)
                var method = manager.GetType().GetMethods()
                    .First(m => m.Name == "GetComponentData" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Entity));
                var getData = method.MakeGenericMethod(typeComponent.GetManagedType());
                return getData.Invoke(manager, new object[] { entity });
            }
        }
    }
#nullable disable        
}