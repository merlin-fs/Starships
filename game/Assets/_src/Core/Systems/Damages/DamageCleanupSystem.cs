using System;
using Unity.Entities;

namespace Game.Model.Weapons
{
    [UpdateInGroup(typeof(GameLogicEndSystemGroup), OrderFirst = true)]
    public partial struct DamageCleanupSystem : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<LastDamage>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public unsafe void OnUpdate(ref SystemState state)
        {
            var job = new CleanupJob()
            {
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct CleanupJob : IJobEntity
        {
            void Execute(ref DynamicBuffer<LastDamage> damages)
            {
                damages.Clear();
            }
        }
    }
}
