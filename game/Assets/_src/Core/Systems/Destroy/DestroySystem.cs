using System;
using Game.Model.Logics;
using Unity.Entities;

namespace Game.Systems
{
    using Model.Stats;

    [UpdateInGroup(typeof(GameEndSystemGroup), OrderLast = true)]
    public partial struct DestroySystem : ISystem
    {
        EntityQuery m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithNone<DeadTag>()
                .WithAspect<Logic.Aspect>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        partial struct SystemJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            private void Execute([EntityIndexInQuery] int idx, in Entity entity, Logic.Aspect logic)
            {
                if (logic.IsCurrentAction(Global.Action.Destroy) && logic.HasWorldState(Global.State.Dead, true))
                    Writer.AddComponent<DeadTag>(idx, entity);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.Dependency = new SystemJob()
            {
                Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
        }
    }
}