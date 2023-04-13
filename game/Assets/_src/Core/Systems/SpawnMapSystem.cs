using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;

namespace Game.Systems
{
    using Model;
    using Model.Worlds;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct SpawnMapSystem : ISystem
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
                .WithAll<NewSpawnMap>()
                .WithAll<SpawnComponent>()
                .Build();

            state.RequireForUpdate(m_Query);
            m_LookupTransform = state.GetComponentLookup<LocalTransform>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            m_LookupTransform.Update(ref state);
            var map = SystemAPI.GetAspectRW<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            var system = state.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            state.Dependency = new SystemJob()
            {
                Map = map,
                LookupTransform = m_LookupTransform,
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
            ecb.DestroyEntity(m_Query);
        }

        partial struct SystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Map;
            [ReadOnly]
            public ComponentLookup<LocalTransform> LookupTransform;
            public EntityCommandBuffer.ParallelWriter Writer;

            public void Execute([EntityIndexInQuery] int idx, in NewSpawnMap spawn, in DynamicBuffer<SpawnComponent> components)
            {
                var inst = Writer.Instantiate(idx, spawn.Prefab);

                var transform = LookupTransform[spawn.Prefab];
                transform.Position = Map.Value.MapToWord(spawn.Position);

                Writer.AddComponent<SpawnTag>(idx, inst);
                Writer.AddComponent(idx, inst, transform);
                foreach (var iter in components)
                    Writer.AddComponent(idx, inst, iter.ComponentType);
            }
        }
    }
}