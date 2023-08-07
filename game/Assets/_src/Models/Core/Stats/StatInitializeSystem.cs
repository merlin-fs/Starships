using System;

using Game.Core.Spawns;

using Unity.Burst;
using Unity.Entities;

namespace Game.Model.Stats
{
    public partial struct Stat
    {
        public struct InitTag : IComponentData { }
    }

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct StatInitializeSystem : ISystem
    {
        private EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat, Stat.InitTag, Spawn.Tag>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnUpdate(ref SystemState state)
        {
            var job = new SystemJob()
            {
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            //state.Dependency.Complete();
        }

        partial struct SystemJob : IJobEntity
        {
            public float Delta;

            void Execute([WithChangeFilter(typeof(Modifier))] StatAspect stats)
            {
                stats.Estimation(Delta);
            }
        }
    }
}