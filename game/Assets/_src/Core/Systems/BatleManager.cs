using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Weapons
{
    using Stats;
    using Logics;

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct WeaponViewSystem : ISystem
    {
        EntityQuery m_Query;
        BufferLookup<Stat> m_LookupStats;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithAll<Logic>()
                .Build();
            state.RequireForUpdate(m_Query);
            m_LookupStats = state.GetBufferLookup<Stat>(false);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            m_LookupStats.Update(ref state);
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new WeaponJob()
            {
                LookupStats = m_LookupStats,
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
        }

        partial struct WeaponJob : IJobEntity
        {
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            public BufferLookup<Stat> LookupStats;
            
            void Execute(in WeaponAspect weapon, in LogicAspect logic)
            {
                if (logic.Equals(Weapon.State.Shoot))
                {
                    if (!LookupStats.HasBuffer(weapon.Target.Value))
                        return;

                    var targetStats = LookupStats[weapon.Target.Value];
                    targetStats.GetRW(GlobalStat.Health).Damage(weapon.Stat(Weapon.Stats.Damage).Value);
                }
            }
        }
    }
}
