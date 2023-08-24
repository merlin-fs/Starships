using System;
using Unity.Entities;
using Unity.Collections;
using Common.Defs;

using Game.Core;

namespace Game.Model.Weapons
{
    using Stats;
    using Logics;

    public partial struct Weapon
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct WeaponSystem : ISystem
        {
            private EntityQuery m_Query;
            private ComponentLookup<Team> m_LookupTeams;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<WeaponAspect>()
                    .WithAspect<Logic.Aspect>()
                    .Build();
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Weapon>());
                m_LookupTeams = state.GetComponentLookup<Team>(true);
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                //var system = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>();
                m_LookupTeams.Update(ref state);
                state.Dependency = new SystemJob()
                {
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    Delta = SystemAPI.Time.DeltaTime,
                    LookupTeams = m_LookupTeams,
                }.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                public EntityCommandBuffer.ParallelWriter Writer;
                [ReadOnly] public ComponentLookup<Team> LookupTeams;

                void Execute([EntityIndexInQuery] int idx, WeaponAspect weapon, Logic.Aspect logic)
                {
                    if (logic.IsCurrentAction(Target.Action.Find))
                    {
                        //UnityEngine.Debug.Log($"{logic.Self} [Logic part] FindOfWeaponTarget set teams {weapon.Unit}");
                        var target = weapon.Target;
                        target.SearchTeams = LookupTeams[weapon.Root].EnemyTeams;
                        target.Radius = weapon.Stat(Stats.Range).Value;
                        weapon.Target = target;
                        return;
                    }

                    if (logic.IsCurrentAction(Action.Reload))
                    {
                        weapon.Time += Delta;
                        if (!(weapon.Time >= weapon.Stat(Stats.ReloadTime).Value)) return;
                        
                        weapon.Time = 0;

                        //TODO: нужно перенести получение кол. патронов...
                        if (!logic.HasWorldState(State.HasAmo, true)) return;
                            
                        var count = (int)weapon.Stat(Stats.ClipSize).Value;
                        logic.SetWorldState(State.NoAmmo, !weapon.Reload(new WriterContext(Writer, idx), count));
                    }

                    if (logic.IsCurrentAction(Action.Shooting))
                    {
                        weapon.Time += Delta;
                        if (weapon.Time >= weapon.Stat(Stats.Rate).Value)
                        {
                            //TODO: Доделать на стороне StateMachine
                            logic.SetAction(EnumHandle.FromEnum(Action.Shoot));
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
                        logic.SetAction(EnumHandle.FromEnum(Action.Shooting));
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