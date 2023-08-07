using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Common.Defs;

using Game.Core.Prefabs;
using Game.Core.Spawns;
using Game.Model;
using Game.Model.Stats;
using Unity.Collections;
using Unity.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Game.Core.Saves
{
    using Converters;


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public sealed class SavedAttribute : Attribute { }

    
    public readonly struct SaveTag: IComponentData
    { }
    public readonly struct LoadTag : IComponentData
    { }

    public readonly struct SavedTag : IComponentData
    { }

    public interface ISavedContext
    {
        string Name { get; }
    }

    public class SaveManager: IDisposable
    {
        private readonly ISavedContext m_Context;
        public SaveManager(ISavedContext context)
        {
            m_Context = context;
        }

        public void Dispose()
        {

        }

        public void Save()
        {
            SaveWorld();
        }

        public void Load() 
        { 
            LoadWorld();
        }

        private void SaveWorld()
        {
            var entityDataPath = Paths.GetPath(m_Context.Name);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(entityDataPath));
            using (var stream = new System.IO.FileStream(entityDataPath, System.IO.FileMode.OpenOrCreate))
            {
                stream.SetLength(0);
                var writer = new System.IO.StreamWriter(stream) { NewLine = "\n" };
                var source = World.DefaultGameObjectInjectionWorld;

                var query = source.EntityManager.CreateEntityQuery(
                    ComponentType.ReadOnly<SavedTag>()
                );
                Write(source, query, writer);
            }
        }

        private void LoadWorld()
        {
            var entityDataPath = Paths.GetPath(m_Context.Name);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(entityDataPath));
            using (var stream = new System.IO.FileStream(entityDataPath, System.IO.FileMode.Open))
            {
                var reader = new System.IO.StreamReader(stream);
                var source = World.DefaultGameObjectInjectionWorld;

                var query = source.EntityManager.CreateEntityQuery(
                    ComponentType.ReadOnly<SavedTag>()
                );
                Read(source, query, reader);
            }
        }

        private class ContractResolver : DefaultContractResolver
        {
            private Type m_Def = typeof(RefLink<>);

            private struct Contract
            {
                public JsonConverter Converter;
                public bool IsRef;
            }

            private Contract m_Default = new Contract { Converter = new DefaultConverter<object>(), IsRef = false };  
                
            private Dictionary<Type, Contract> m_Converters = new Dictionary<Type, Contract>() 
            {
                {typeof(Entity), new Contract{Converter = new EntityConverter(), IsRef = true}},
                {typeof(PrefabInfo), new Contract{Converter = new PrefabConverter(), IsRef = false}},
                {typeof(DynamicBuffer<>), new Contract{Converter = new DynamicBufferConverter(), IsRef = false}},
                {typeof(Stat), new Contract{Converter = new StatConverter(), IsRef = false}},
                {typeof(ModifierConverter), new Contract{Converter = new ModifierConverter(), IsRef = false}},
            };

            private IEnumerable<MemberInfo> GetFields(IReflect type)
            {
                var list = type?.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Cast<MemberInfo>();
                return list;
            }
            
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                return GetFields(objectType).ToList();
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                property.IsReference = false;
                if (member is FieldInfo info &&
                    m_Converters.TryGetValue(info.FieldType, out var contract))
                {
                    property.Readable = true;
                    property.Converter = contract.Converter;
                    property.IsReference = contract.IsRef;
                }
                return property;                
            }
            
            protected override JsonContract CreateContract(Type type)
            {
                JsonContract contract = base.CreateContract(type);
                contract.IsReference = false;
                if (m_Converters.TryGetValue(type, out var value))
                {
                    contract.Converter = value.Converter;
                    contract.IsReference = value.IsRef;
                }
                else if (m_Default.Converter.CanConvert(type))
                {
                    contract.Converter = m_Default.Converter;
                    contract.IsReference = m_Default.IsRef;
                }
                return contract;
            }
        }
        
        public class GenericResolver<TEntity> : IReferenceResolver
        {
            private IReferenceResolver m_DefaultResolver;
            
            private readonly IDictionary<string, TEntity> _objects = new Dictionary<string, TEntity>();
            private readonly Func<TEntity, string> _keyReader;

            public GenericResolver(Func<TEntity, string> keyReader)
            {
                _keyReader = keyReader;
            }

            public object ResolveReference(object context, string reference)
            {
                TEntity o;
                if (_objects.TryGetValue(reference, out o))
                {
                    return null;
                }

                return null;
            }

            public string GetReference(object context, object value)
            {
                if (value is not TEntity entity) return null;
                var key = _keyReader(entity);
                _objects[key] = entity;
                return key;
            }

            public bool IsReferenced(object context, object value)
            {
                /*
                if (value is not TEntity entity) return false;
                var key = _keyReader(entity);
                if (_objects.ContainsKey(key))
                    return true;
                _objects[key] = entity;
                return false;
                */
                //m_DefaultResolver.IsReferenced(context, value)
                return value is TEntity entity && _objects.ContainsKey(_keyReader(entity));
            }

            public void AddReference(object context, string reference, object value)
            {
                if(value is TEntity)
                    _objects[reference] = (TEntity)value;
            }
        }

        private JsonSerializer GetSerializer(EntityManager entityManager)
        {
            var settings = new JsonSerializerSettings  
            {  
                Formatting = Formatting.Indented,  
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Context = new StreamingContext(StreamingContextStates.File, entityManager),
                ContractResolver = new ContractResolver(),
                ReferenceResolverProvider = () =>new GenericResolver<Entity>(p => p.Index.ToString()), 
            };
            //settings.Converters.Add(new DefaultConverter<object>());
            //settings.Converters.Add(new PrefabConverter());
            JsonConvert.DefaultSettings = () => settings;
            return JsonSerializer.Create(settings);
        }

        struct LoadData
        {
            [JsonProperty("$id")]
            public int id;
            public string ConfigID;
        }
        
        private void Read(World source, EntityQuery query, StreamReader reader)
        {
            var manager = source.EntityManager;
            var serializer = GetSerializer(manager);
            using JsonReader jr = new JsonTextReader(reader);
            JArray data = serializer.Deserialize<JArray>(jr);

            var ecb = manager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
                .CreateCommandBuffer()
                .AsParallelWriter();
            var archetype = manager.CreateArchetype(ComponentType.ReadWrite<Spawn.Load>(),
                ComponentType.ReadWrite<Spawn.Component>());

            using var entities = manager.CreateEntity(archetype, data.Count, Allocator.Temp);
            for (int i = 0; i < data.Count; i++)
            //Parallel.For(0, data.Count, i =>
            {
                var entity = entities[i];
                var link = new RefLink<JToken>(GCHandle.Alloc(data[i], GCHandleType.Pinned));
                ecb.AppendToBuffer<Spawn.Component>(i, entity, ComponentType.ReadOnly<SavedTag>());
                ecb.SetComponent(i, entity, new Spawn.Load
                {
                    Data = link,
                    ID = data[i].Value<int>("$id")
                });
            }

            ;//);
        }

        private void Write(World source, EntityQuery query, StreamWriter writer)
        {
            var serializer = GetSerializer(source.EntityManager);
            using var entities = query.ToEntityArray(Allocator.Temp);
            using JsonWriter jw = new JsonTextWriter(writer);
            serializer.Serialize(jw, entities);
        }
    }
}
