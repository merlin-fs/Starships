using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Logics;
    using Result = Logics.Logic.Result;

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct WeaponSystem : ISystem
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

            void Execute([EntityIndexInQuery] int entityIndexInQuery, ref WeaponAspect weapon, ref LogicAspect logic)
            {
                if (logic.Equals(Weapon.State.Shooting))
                {
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Config.Rate.Value)
                    {
                        weapon.Time = 0;
                        weapon.Shot();
                        if (weapon.Count == 0)
                            logic.SetResult(Result.Done);
                    }
                }
                
                if (logic.Equals(Weapon.State.Sleep))
                {
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Config.ReloadTime.Value)
                    {
                        weapon.Time = 0;
                        logic.SetResult(Result.Done);
                    }
                }

                if (logic.Equals(Weapon.State.Reload))
                {
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Config.ReloadTime.Value)
                    {
                        weapon.Time = 0;
                        weapon.Reload(new DefExt.WriterContext(Writer, entityIndexInQuery));
                        if (weapon.Count == 0)
                            logic.SetResult(Result.Error);
                        else
                            logic.SetResult(Result.Done);
                    }
                }
            }
        }
    }
}
