using System;

using Common.Core;

using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;

using Game.Core.Prefabs;
using Game.Model;
using Game.Model.Worlds;

using Newtonsoft.Json.Linq;

using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;

using Unity.Transforms;

using UnityEngine;
using UnityEngine.Pool;

using Object = UnityEngine.Object;

namespace Game.Core.Spawns
{
    public partial struct Spawn
    {
        public class SpawnViewPool : ObjectPool<SpawnView>
        {
            public SpawnViewPool(Transform parent, Container container)
                : base(
                        () =>
                        {
                            var newObj = new GameObject("spawner").AddComponent<SpawnView>();
                            AttributeInjector.Inject(newObj, container);
                            newObj.transform.parent = parent;
                            return newObj;
                        },
                        view => view.gameObject.SetActive(true),
                        view => view.gameObject.SetActive(false),
                        view => Object.Destroy(view.gameObject),
                        true)
            {}
        } 
        
        public class SpawnView : MonoBehaviour
        {
            [Inject] private SpawnViewPool m_Pool;
            private Container m_Container;

            public void SetContext(Container container)
            {
                m_Container = container;
            }
            private void Update()
            {
                var view = m_Container.Resolve<GameObject>();
                m_Pool.Release(this);
            }
        }

        public class Spawner
        {
            [Inject] private SpawnViewPool m_Pool;
            private readonly Container m_Container;
            private readonly IConfig m_Config;
            private readonly EntityCommandBuffer m_Ecb;
            private Entity m_Entity;

            private Spawner(IConfig config, EntityCommandBuffer ecb, Container container)
            {
                AttributeInjector.Inject(this, container);
                m_Config = config;
                m_Container = container;
                m_Ecb = ecb;
                if (m_Config.EntityPrefab == Entity.Null) throw new ArgumentNullException($"EntityPrefab {m_Config.ID} not assigned");
            }
                
            public class Builder
            {
                private readonly Spawner m_Spawner;
                
                public Builder(Spawner spawner)
                {
                    m_Spawner = spawner;
                }
                
                public Builder WithView()
                {
                    m_Spawner.CreateContext();
                    return this;
                }

                public Builder WithComponents(DynamicBuffer<Component> components)
                {
                    foreach (var iter in components)
                        m_Spawner.m_Ecb.AddComponent(m_Spawner.m_Entity, iter.ComponentType);
                    return this;
                }

                public Builder WithComponent(object component, Type type)
                {
                    m_Spawner.m_Ecb.AddComponent(m_Spawner.m_Entity, component, type);
                    return this;
                }

                public Builder WithComponent<T>()
                    where T : unmanaged, IComponentData
                {
                    m_Spawner.m_Ecb.AddComponent<T>(m_Spawner.m_Entity);
                    return this;
                }

                public Builder WithComponent<T>(T component)
                    where T: unmanaged, IComponentData 
                {
                    m_Spawner.m_Ecb.AddComponent<T>(m_Spawner.m_Entity, component);
                    return this;
                }

                public Builder WithPosition(Map.Data map, int2 position)
                {
                    m_Spawner.m_Ecb.AddComponent(m_Spawner.m_Entity, new Move{ Position = map.MapToWord(position) });
                    return this;
                }

                public Builder WithData(JToken token)
                {
                    foreach (var iter in token)
                    {
                        var type = Type.GetType(((JProperty)iter).Name);
                        if (TypeManager.IsSystemType(type) || type == typeof(PrefabInfo))
                            continue;
                        var component = iter.ToObject(type);
                        if (component != null)
                            this.WithComponent(component, type);
                    }

                    return this;
                }
            }

            public class RequestBuilder
            {
                private readonly Spawner m_Spawner;
                public RequestBuilder(Spawner spawner)
                {
                    m_Spawner = spawner;
                }

                public RequestBuilder WithComponent<T>()
                    where T: unmanaged, IComponentData 
                {
                    m_Spawner.m_Ecb.AppendToBuffer<Spawn.Component>(m_Spawner.m_Entity, ComponentType.ReadOnly<T>());
                    return this;
                }
                
            }
            
            public static Builder Spawn(IConfig config, EntityCommandBuffer ecb, Container container)
            {
                var spawner = new Spawner(config, ecb, container); 
                var builder = new Builder(spawner);

                spawner.m_Entity = spawner.CreateEntity();
                builder.WithComponent<Tag>();

                Debug.Log($"[Spawner] spawn: {config.ID} ({spawner.m_Entity})");
                return builder;
            }

            public static RequestBuilder SpawnRequest(IConfig config, EntityCommandBuffer ecb, Container container)
            {
                var spawner = new Spawner(config, ecb, container); 
                var builder = new RequestBuilder(spawner);
                spawner.m_Entity = ecb.CreateEntity();
                ecb.AddBuffer<Spawn.Component>(spawner.m_Entity);
                ecb.AddComponent<Spawn>(spawner.m_Entity, new Spawn{PrefabID = config.ID});

                Debug.Log($"[Spawner] request: {config.ID} ({spawner.m_Entity})");
                
                return builder;
            }

            public static RequestBuilder SpawnRequest(IConfig config, Entity entity, JToken token, EntityCommandBuffer ecb, Container container)
            {
                var spawner = new Spawner(config, ecb, container); 
                var builder = new RequestBuilder(spawner);
                
                spawner.m_Entity = entity;
                var link = RefLink<JToken>.From(token);
                var id = token.Value<int>("$id");
                
                ecb.AddComponent<Spawn>(spawner.m_Entity, new Spawn{ID = id, PrefabID = config.ID, Data = link});

                Debug.Log($"[Spawner] spawn from data: {config.ID} ({spawner.m_Entity})");
                return builder;
            }

            private Entity CreateEntity()
            {
                var entity = m_Ecb.Instantiate(m_Config.EntityPrefab);
                m_Ecb.AddComponent<Tag>(entity);
                return entity;
            }

            private async void CreateContext()
            {
                if (m_Config is not IViewPrefab viewPrefab) throw new NotImplementedException($"IViewPrefab {m_Config.ID} NotImplemented");
                var prefab = await viewPrefab.GetViewPrefab();
                if (!prefab) throw new ArgumentNullException($"ViewPrefab {m_Config.ID} not assigned");
                
                var newContext = m_Container.Scope(builder =>
                {
                    builder.AddSingleton(container =>
                    {
                        var inst = GameObject.Instantiate(prefab);
                        AttributeInjector.Inject(inst, container);
                        return inst;
                    });
                });
                m_Ecb.AddComponent(m_Entity, new PrefabInfo.ContextReference{Value = newContext});
                m_Ecb.AddComponent<LocalTransform>(m_Entity);
                m_Pool.Get().SetContext(newContext);
            }
        }
   }
}