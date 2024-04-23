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
        [UpdateInGroup(typeof(GameLogicObjectSystemGroup))]
        public partial struct System : ISystem
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
                    /* logic
                    if (logic.IsCurrentAction(Global.Action.Init))
                    {
                        UnityEngine.Debug.Log($"{weapon.Self} [Weapon] init");
                        var team = LookupTeams[weapon.Root];
                        var query = new Target.Query {
                            Radius = weapon.Stat(Stats.Range).Value, 
                            SearchTeams = team.EnemyTeams,
                        };
                        Writer.SetComponent(idx, weapon.Self, query);
                    }

                    if (logic.IsCurrentAction(Action.Reload))
                    {
                        weapon.Time += Delta;
                        if (!(weapon.Time >= weapon.Stat(Stats.ReloadTime).Value)) return;
                        
                        weapon.Time = 0;

                        //TODO: нужно перенести получение кол. патронов...
                        //if (!logic.HasWorldState(State.HasAmo, true)) return;
                            
                        var count = (int)weapon.Stat(Stats.ClipSize).Value;
                        logic.SetWorldState(State.HasAmo, weapon.Reload(new WriterContext(Writer, idx), count));
                    }

                    if (logic.IsCurrentAction(Action.Attack))
                    {
                        weapon.Time += Delta;
                        if (!(weapon.Time >= weapon.Stat(Stats.Rate).Value)) return;
                        
                        logic.SetWorldState(State.Shooting, true);
                        weapon.Time = 0;
                        weapon.Shot();
                        
                        if (weapon.Count != 0) return;
                        logic.SetWorldState(State.HasAmo, false);
                        return;
                    }

                    if (logic.IsCurrentAction(Action.Shoot))
                    {
                        logic.SetWorldState(State.Shooting, false);
                        return;
                    }

                    if (logic.IsCurrentAction(Global.Action.Destroy))
                    {
                        logic.SetWorldState(Global.State.Dead, true);
                    }
                    **/
                }
            }
        }
    }
}