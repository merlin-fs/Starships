using System;
using Unity.Entities;

namespace Game.Model.Weapons
{
    public partial struct Damage
    {
        [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
        [UpdateAfter(typeof(DamageProcessSystem))]
        partial struct DamageCleanupSystem : ISystem
        {
            EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAllRW<LastDamage>()
                    .Build();
                state.RequireForUpdate(m_Query);
            }

            public void OnDestroy(ref SystemState state) { }

            public unsafe void OnUpdate(ref SystemState state)
            {
                var job = new SystemJob()
                {
                };
                state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
                //state.Dependency.Complete();
            }

            partial struct SystemJob : IJobEntity
            {
                public void Execute(ref DynamicBuffer<LastDamage> damages)
                {
                    damages.Clear();
                }
            }
        }
    }
}
