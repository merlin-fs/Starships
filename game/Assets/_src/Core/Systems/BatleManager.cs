using System;
using Unity.Entities;

namespace Game.Model.Weapons
{
    using Stats;
    using Logics;
    
    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct WeaponViewSystem : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithAll<Logic>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            //Stats.Stat.

            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new WeaponJob()
            {
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct WeaponJob : IJobEntity
        {
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, in WeaponAspect weapon, in LogicAspect logic)
            {
                if (logic.Equals(Weapon.State.Shoot))
                {
                    //weapon.Target
                    return;
                }
            }
        }
    }
}
