using System;
using Unity.Entities;
    
namespace Game.Model.Stats
{
    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Stat>());
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new HealthJob()
            {
                Writer = ecb.AsParallelWriter(),
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct HealthJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in DynamicBuffer<Stat> stats)
            {
                if (stats.TryGetStat(GlobalStat.Health, out Stat health) &&
                    health.Value <= 0)
                {
                    Writer.AddComponent<DeadTag>(idx, entity);
                }
            }
        }
    }
}