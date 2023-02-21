using System;
using Unity.Entities;
using Unity.Collections;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Stats;
    using Logics;

    public partial struct Weapon
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct WeaponSystem : ISystem
        {
            EntityQuery m_Query;
            ComponentLookup<Team> m_LookupTeams;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Weapon>()
                    .WithAll<Logic>()
                    .Build();
                state.RequireForUpdate(m_Query);
                m_LookupTeams = state.GetComponentLookup<Team>(false);
            }

            public void OnDestroy(ref SystemState state) { }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupTeams.Update(ref state);
                var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
                var job = new SystemJob()
                {
                    Teams = m_LookupTeams,
                    Writer = ecb.AsParallelWriter(),
                    Delta = SystemAPI.Time.DeltaTime,
                };
                state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
                state.Dependency.Complete();
            }

            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                [ReadOnly] public ComponentLookup<Team> Teams;
                public EntityCommandBuffer.ParallelWriter Writer;

                public void Execute([EntityIndexInQuery] int idx, ref WeaponAspect weapon, ref Logic.Aspect logic)
                {

                    if (logic.IsCurrentAction(Action.Reload))
                    {
                        weapon.Time += Delta;
                        if (weapon.Time >= weapon.Stat(Stats.ReloadTime).Value)
                        {
                            weapon.Time = 0;

                            //TODO: нужно перенести получение кол. патронов...
                            if (logic.HasWorldState(State.HasAmo, true))
                            {
                                var count = (int)weapon.Stat(Stats.ClipSize).Value;
                                logic.SetWorldState(State.NoAmmo,
                                    !weapon.Reload(new DefExt.WriterContext(Writer, idx), count));
                            }
                        }
                    }

                    if (logic.IsCurrentAction(Action.Shooting))
                    {
                        weapon.Time += Delta;
                        if (weapon.Time >= weapon.Stat(Stats.Rate).Value)
                        {
                            //TODO: Доделать на стороне StateMachine
                            logic.SetAction(LogicHandle.FromEnum(Action.Shoot));
                            weapon.Time = 0;
                            weapon.Shot();
                            if (weapon.Count == 0)
                            {
                                logic.SetWorldState(State.NoAmmo, true);
                                logic.SetWorldState(Target.State.Dead, false);
                            }
                        }
                        return;
                    }

                    if (logic.IsCurrentAction(Action.Shoot))
                    {
                        //TODO: Доделать на стороне StateMachine
                        logic.SetAction(LogicHandle.FromEnum(Action.Shooting));
                        return;
                    }

                    if (logic.IsCurrentAction(Global.Action.Destroy))
                    {
                        logic.SetWorldState(Global.State.Dead, true);
                    }
                }
            }
        }
    }
}