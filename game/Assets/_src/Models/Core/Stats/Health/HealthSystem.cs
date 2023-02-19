using System;
using Unity.Entities;
    
namespace Game.Model.Stats
{
    using Logics;

    [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithAll<Logic>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Stat>());
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var system = state.World.GetOrCreateSystemManaged<GameLogicEndCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            var job = new SystemJob()
            {
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct SystemJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            public void Execute([EntityIndexInQuery] int idx, in Entity entity, in DynamicBuffer<Stat> stats, ref Logic.Aspect logic)
            {
                if (stats.TryGetStat(Global.Stat.Health, out Stat health) && health.Value <= 0)
                {
                    //UnityEngine.Debug.Log($"{logic.Self} [Health system] set Destroy");
                    logic.SetEvent(Global.Action.Destroy);
                }
            }
        }
    }
}