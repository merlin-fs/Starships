using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Game.Model.Worlds;

namespace Game.Core.Spawns
{
    public partial struct Spawn
    {
        [UpdateInGroup(typeof(GameSpawnSystemGroup))]
        partial struct System : ISystem
        {
            EntityQuery m_Query;
            EntityQuery m_QueryMap;
            ComponentLookup<LocalTransform> m_LookupTransform;

            public void OnCreate(ref SystemState state)
            {
                m_QueryMap = SystemAPI.QueryBuilder()
                    .WithAspect<Map.Aspect>()
                    .Build();

                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Spawn, Component>()
                    .Build();
                state.RequireForUpdate(m_Query);
                m_LookupTransform = state.GetComponentLookup<LocalTransform>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupTransform.Update(ref state);
                var map = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
                var system = state.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
                
                state.Dependency = new SystemJob
                {
                    Map = map, 
                    LookupTransform = m_LookupTransform, 
                    Writer = system.CreateCommandBuffer().AsParallelWriter(),
                }.ScheduleParallel(m_Query, state.Dependency);
                //state.Dependency.Complete();
                var ecb = system.CreateCommandBuffer();
                ecb.DestroyEntity(m_Query, EntityQueryCaptureMode.AtPlayback);
            }

            partial struct SystemJob : IJobEntity
            {
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Map.Aspect Map;

                [ReadOnly] public ComponentLookup<LocalTransform> LookupTransform;
                public EntityCommandBuffer.ParallelWriter Writer;

                void Execute([EntityIndexInQuery] int idx, in Spawn spawn, in DynamicBuffer<Component> components)
                {
                    var inst = Writer.Instantiate(idx, spawn.Prefab);

                    var transform = LookupTransform[spawn.Prefab];
                    transform.Position = Map.Value.MapToWord(spawn.Position);

                    Writer.AddComponent<Tag>(idx, inst);
                    Writer.AddComponent(idx, inst, transform);
                    foreach (var iter in components)
                        Writer.AddComponent(idx, inst, iter.ComponentType);
                }
            }
        }
    }
}