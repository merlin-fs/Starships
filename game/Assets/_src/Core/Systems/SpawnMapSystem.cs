using System;
using Common.Core;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using Unity.Collections;

namespace Game.Model
{
    public struct SpawnMapTag : IComponentData
    {
        public Entity Prefab;
        public int2 Position;
        public ObjectID ConfigID;
    }
}

namespace Game.Systems
{
    using Game.Model;
    using Game.Model.Worlds;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct SpawnMapSystem : ISystem
    {
        EntityQuery m_Query;
        EntityQuery m_QueryMap;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SpawnMapTag>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var map = SystemAPI.GetAspectRW<Map.Aspect>(m_QueryMap.GetSingletonEntity());

            var system = state.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            state.Dependency = new SystemJob()
            {
                Map = map,
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
            //system.AddJobHandleForProducer(state.Dependency);
            state.Dependency.Complete();
            ecb.DestroyEntity(m_Query);
        }

        partial struct SystemJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Map;

            public void Execute([EntityIndexInQuery] int idx, in Entity entity, in SpawnMapTag spawn)
            {
                var inst = Writer.Instantiate(idx, spawn.Prefab);
                Writer.AddComponent<Spawn>(idx, inst);
                var pos = LocalTransform.FromPosition(Map.Value.MapToWord(spawn.Position));
                pos.Scale = 0.5f;
                Writer.AddComponent(idx, inst, pos);

                Map.SetObject(spawn.Position, inst);
            }
        }
    }
}