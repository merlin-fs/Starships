using System;
using Unity.Entities;

namespace Game.Systems
{
    using Model.Stats;

    //[UpdateInGroup(typeof(GameEndSystemGroup), OrderLast = true)]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial struct CleanupSystem : ISystem
    {
        EntityQuery m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<DeadTag>()
                .WithNone<WaitTag>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        partial struct DeadJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            void Execute([EntityIndexInQuery] int idx, in Entity entity)
            {
                UnityEngine.Debug.Log($"[{entity}] Destroy");
                Writer.DestroyEntity(idx, entity);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var system = state.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            state.Dependency = new DeadJob()
            {
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
            //state.Dependency.Complete();
            system.AddJobHandleForProducer(state.Dependency);
        }
    }
}