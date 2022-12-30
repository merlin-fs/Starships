using System;
using Unity.Entities;
using Common.Defs;
using Unity.Collections;

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
                Teams = state.GetComponentLookup<Team>(false),
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct WeaponJob : IJobEntity
        {
            public float Delta;
            [ReadOnly]
            public ComponentLookup<Team> Teams;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int entityIndexInQuery, ref WeaponAspect weapon, ref LogicAspect logic)
            {
                if (logic.Equals(Target.State.Find))
                {
                    if (weapon.Unit != Entity.Null)
                    {
                        weapon.SoughtTeams = Teams[weapon.Unit].EnemyTeams;
                        UnityEngine.Debug.Log($"[{weapon.Self}]Set find {weapon.SoughtTeams}");
                    }
                    else
                    {
                        logic.SetResult(Result.Error);
                    }
                    return;
                }

                if (logic.Equals(Weapon.State.Shooting))
                {
                    if (weapon.Count == 0)
                    {
                        logic.SetResult(Result.Error);
                        return;
                    }
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Config.Rate.Value)
                    {
                        weapon.Time = 0;
                        weapon.Shot();
                        logic.SetResult(Result.Done);
                    }
                    return;
                }

                if (logic.Equals(Weapon.State.Sleep))
                {
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Config.ReloadTime.Value)
                    {
                        weapon.Time = 0;
                        logic.SetResult(Result.Done);
                    }
                    return;
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
                    return;
                }
            }
        }
    }
}
