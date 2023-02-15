using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Logics;
    using Stats;

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
                    .WithNone<DeadTag>()
                    .Build();
                state.RequireForUpdate(m_Query);
                m_LookupTeams = state.GetComponentLookup<Team>(false);
            }

            public void OnDestroy(ref SystemState state) { }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupTeams.Update(ref state);
                var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
                var job = new WeaponJob()
                {
                    Teams = m_LookupTeams,
                    Writer = ecb.AsParallelWriter(),
                    Delta = SystemAPI.Time.DeltaTime,
                };
                state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
                state.Dependency.Complete();
            }

            partial struct WeaponJob : IJobEntity
            {
                public float Delta;
                [ReadOnly] public ComponentLookup<Team> Teams;
                public EntityCommandBuffer.ParallelWriter Writer;

                void Execute([EntityIndexInQuery] int idx, ref WeaponAspect weapon, 
                    ref LogicAspect logic)
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
                            logic.SetAction(LogicHandle.FromEnum(Action.Shoot));
                            weapon.Time = 0;
                            weapon.Shot(new DefExt.WriterContext(Writer, idx));
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
                        logic.SetAction(LogicHandle.FromEnum(Action.Shooting));
                        return;
                    }


                    /*
                    switch (logic.CurrentAction)
                    {
                        case Weapon.State.Shooting:
                            //TODO: Перенести в отдельный system "Turret"
                            var direction = weapon.Target.WorldTransform.Position;
                            direction = transform.TransformPointWorldToParent(direction) - transform.LocalPosition;
                            transform.LocalRotation = math.nlerp(
                                transform.LocalRotation,
                                quaternion.LookRotationSafe(direction, math.up()),
                                weapon.Time + Delta * 10f);

                            if (weapon.Count == 0)
                            {
                                logic.TrySetResult(Weapon.Condition.NoAmmo);
                                return;
                            }

                            weapon.Time += Delta;
                            if (weapon.Time >= weapon.Stat(Weapon.Stats.Rate).Value)
                            {
                                weapon.Time = 0;
                                //!!!
                                logic.TrySetResult(Weapon.Condition.NoAmmo);
                            }
                            break;

                        case Weapon.State.Sleep:
                            weapon.Time += Delta;
                            if (weapon.Time >= weapon.Stat(Weapon.Stats.ReloadTime).Value)
                            {
                                weapon.Time = 0;
                                logic.TrySetResult(logic.Result);
                            }
                            break;
                    }
                    */
                }
            }
        }
    }
}