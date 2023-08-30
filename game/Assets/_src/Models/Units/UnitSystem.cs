using System;
using Unity.Entities;
using Unity.Collections;

using Game.Model.Logics;
using Game.Model.Stats;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct System : ISystem
        {
            private EntityQuery m_Query;
            private ComponentLookup<Team> m_LookupTeams;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<UnitAspect>()
                    .WithAspect<Logic.Aspect>()
                    .Build();
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
                m_LookupTeams = state.GetComponentLookup<Team>(true);
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
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

                void Execute([EntityIndexInQuery] int idx, Logic.Aspect logic, UnitAspect unit)
                {
                    if (logic.IsCurrentAction(Global.Action.Init))
                    {
                        UnityEngine.Debug.Log($"{unit.Self} [Unit] init");
                        var team = LookupTeams[unit.Self];
                        var query = new Target.Query {
                            Radius = float.MaxValue, 
                            SearchTeams = team.EnemyTeams,
                        };
                        Writer.SetComponent(idx, unit.Self, query);
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