using System;
using System.Threading.Tasks;
using Unity.Entities;

namespace Game.Model.Stats
{
    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct StatSystem : ISystem
    {
        EntityQuery m_Query;
        float m_Time;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithAll<Modifier>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Modifier>());

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state)
        {

        }

        partial struct StatJob : IJobEntity
        {
            public uint LastSystemVersion;
            public float Delta;

            void Execute([WithChangeFilter(typeof(Modifier))] ref DynamicBuffer<Stat> _stats, in ModifiersAspect _aspect)
            {
                var aspect = _aspect;
                var stats = _stats;
                var delta = Delta;

                Parallel.For(0, stats.Length,
                    (i) =>
                    {
                        Change(i);
                    });

                void Change(int i)
                {
                    var stat = stats[i];
                    aspect.Estimation(ref stat, delta);
                    stats[i] = stat;
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            /*
            m_Time += SystemAPI.Time.DeltaTime;
            if (m_Time < 1f)
                return;
            m_Time = 0;
            */

            var job = new StatJob()
            {
                LastSystemVersion = state.LastSystemVersion,
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }
    }
}