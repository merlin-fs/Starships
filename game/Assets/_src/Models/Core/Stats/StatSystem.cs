using System;
using Unity.Burst;
using Unity.Entities;

namespace Game.Model.Stats
{
    [UpdateInGroup(typeof(GameLogicDoneSystemGroup), OrderFirst = true)]
    public partial struct StatSystem : ISystem
    {
        private EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithAll<Modifier>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Stat>());
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        partial struct StatJob : IJobEntity
        {
            public float Delta;

            void Execute([WithChangeFilter(typeof(Modifier))] ref StatAspect stats)
            {
                stats.Estimation(Delta);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var job = new StatJob()
            {
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }
    }
}