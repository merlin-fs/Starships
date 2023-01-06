using System;
using Unity.Entities;
using Common.Defs;
using Unity.Collections;

namespace Game.Views.Weapons
{
    using Game.Model.Stats;

    using Model.Logics;
    using Model.Weapons;

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct WeaponViewSystem : ISystem
    {
        EntityQuery m_Query;
               
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
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
                    Writer.AddComponent(idx, weapon.Self, (Game.Views.Particle)"shot");
                    return;
                }
            }
        }
    }
}
