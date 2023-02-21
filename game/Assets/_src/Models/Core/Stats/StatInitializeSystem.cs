using System;
using Unity.Burst;
using Unity.Entities;

using static Game.Model.Stats.Global;
using static UnityEngine.EventSystems.EventTrigger;

namespace Game.Model.Stats
{
    public struct StatInit : IComponentData { }


    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial struct StatInitializeSystem : ISystem
    {
        private EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithAll<Spawn>()
                .WithAll<StatInit>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        partial struct SystemJob : IJobEntity
        {
            public float Delta;

            public void Execute([WithChangeFilter(typeof(Modifier))] ref StatAspect stats)
            {
                stats.Estimation(Delta);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var job = new SystemJob()
            {
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }
    }
}