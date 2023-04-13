using System;
using Unity.Entities;

namespace Game.Systems
{
    using Model.Stats;

    [UpdateInGroup(typeof(GameSpawnSystemGroup), OrderLast = true)]
    public partial struct DestroyCleanupSystem : ISystem
    {
        EntityQuery m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<DeadTag>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        partial struct SystemJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            public void Execute([EntityIndexInQuery] int idx, in Entity entity)
            {
                UnityEngine.Debug.Log($"{entity} [Cleanup] destroy");
                Writer.DestroyEntity(idx, entity);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var system = state.World.GetOrCreateSystemManaged<GameLogicEndCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            state.Dependency = new SystemJob()
            {
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }
    }
}