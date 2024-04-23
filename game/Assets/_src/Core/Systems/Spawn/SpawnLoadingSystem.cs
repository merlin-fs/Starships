using System;

using Common.Defs;

using Game.Core.Repositories;
using Game.Core.Prefabs;
using Unity.Entities;
using Newtonsoft.Json.Linq;

using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;

using UnityEngine;

namespace Game.Core.Spawns
{
    public partial struct Spawn
    {
        /*
        [UpdateInGroup(typeof(GameSpawnSystemGroup))]
        partial struct LoadingSystem : ISystem
        {
            private EntityQuery m_Query;
            private static string m_PrefabType;
            [Inject] private static ObjectRepository m_Repository;
            [Inject] private static Container m_Container;
            
            public void OnCreate(ref SystemState state)
            {
                m_PrefabType = typeof(PrefabInfo).FullName;
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Load, Component>()
                    .Build();
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
                var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

                foreach (var (spawn, entity) in SystemAPI.Query<Load>().WithEntityAccess())
                {
                    var components = SystemAPI.GetBuffer<Component>(entity);
                    var configId = spawn.Data.Value.Value<string>(m_PrefabType);
                    var config = m_Repository.FindByID(configId);
                    if (config == null) throw new ArgumentNullException($"Prefab {configId} not found");

                    var builder = Builder.Spawn(config, ecb, m_Container)
                        .WithView()
                        .WithComponents(components);

                    foreach (var iter in spawn.Data.Value)
                    {
                        var type = Type.GetType(((JProperty)iter).Name);
                        if (TypeManager.IsSystemType(type) || type == typeof(PrefabInfo))
                            continue;
                        var component = iter.ToObject(type);
                        if (component != null)
                            builder.WithComponent(component, type);
                    }
                    //!!!Writer.AddComponent<ProjectDawn.Navigation.DrawGizmos>(idx, inst);
                    ecb.DestroyEntity(entity);
                }

                /*
                state.Dependency = new SpawnJob
                {
                    Writer = ecb.AsParallelWriter(),
                }.ScheduleParallel(m_Query, state.Dependency);
                */
            }

            /*
            partial struct SpawnJob : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter Writer;

                void Execute([EntityIndexInQuery] int idx, in Entity entity, in Load spawn,
                    in DynamicBuffer<Component> components)
                {
                    var configId = spawn.Data.Value.Value<string>(m_PrefabType);
                    var config = m_Repository.FindByID(configId);
                    if (config == null) throw new ArgumentNullException($"Prefab {configId} not found");
                    if (config.EntityPrefab == Entity.Null) throw new ArgumentNullException($"Prefab {configId} not assigned");

                    var inst = Writer.Instantiate(idx, config.EntityPrefab);
                    if (config is IViewPrefab viewPrefab)
                    {
                        var prefab = viewPrefab.GetViewPrefab();
                        var newContext = m_Container.Scope(builder =>
                        {
                            builder.AddTransient(container =>
                            {
                                var inst = GameObject.Instantiate(prefab);
                                AttributeInjector.Inject(inst, container);
                                return inst;
                            });
                        });
                        EntityCommandBufferManagedComponentExtensions
                            .AddComponent(Writer, idx, entity, new PrefabInfo.ViewReference{Value = newContext});
                    }

                    foreach (var iter in spawn.Data.Value)
                    {
                        var type = Type.GetType(((JProperty)iter).Name);
                        if (TypeManager.IsSystemType(type))
                            continue;
                        var component = iter.ToObject(type);
                        if (component != null)
                            Writer.AddComponent(idx, inst, component, type);
                    }
                    Writer.AddComponent<Tag>(idx, inst);
                    foreach (var iter in components)
                        Writer.AddComponent(idx, inst, iter.ComponentType);

                    //!!!Writer.AddComponent<ProjectDawn.Navigation.DrawGizmos>(idx, inst);
                    Writer.DestroyEntity(idx, entity);
                }
            }
        }
    }
        */
}