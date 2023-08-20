using System;
using Common.Core;
using Game.Core.Prefabs;
using Game.Core.Repositories;
using Game.Core.Saves;
using Game.Views.Stats;
using Unity.Entities;
using Unity.Collections;
using Newtonsoft.Json.Linq;

namespace Game.Core.Spawns
{
    public partial struct Spawn
    {
        [UpdateInGroup(typeof(GameSpawnSystemGroup))]
        partial struct LoadingSystem : ISystem
        {
            private EntityQuery m_Query;
            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Load, Component>()
                    .Build();
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
                var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

                state.Dependency = new SpawnJob
                {
                    Writer = ecb.AsParallelWriter(),
                }.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct SpawnJob : IJobEntity
            {
                private DiContext.Var<ObjectRepository> m_Repository;
                public EntityCommandBuffer.ParallelWriter Writer;

                void Execute([EntityIndexInQuery] int idx, in Entity entity, in Load spawn,
                    in DynamicBuffer<Component> components)
                {
                    var prefabIndex = TypeManager.GetTypeIndex<PrefabInfo>();
                    var configId = spawn.Data.Value.Value<string>(prefabIndex.ToString());
                    var config = m_Repository.Value.FindByID(configId);

                    var inst = Writer.Instantiate(idx, config.Prefab);

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

                    Writer.DestroyEntity(idx, entity);
                }
            }
        }
    }
}