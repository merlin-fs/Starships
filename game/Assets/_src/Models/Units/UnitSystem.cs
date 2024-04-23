using System;
using Unity.Entities;
using Unity.Collections;

using Game.Model.Logics;
using Game.Model.Stats;
using Game.Model.Worlds;

using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        [UpdateInGroup(typeof(GameLogicObjectSystemGroup))]
        public partial struct System : ISystem
        {
            private EntityQuery m_Query;
            private ComponentLookup<Team> m_LookupTeams;
            private Logic.Aspect.Lookup m_LookupLogicAspect;
            private ComponentLookup<Map.Move> m_LookupMapTransform;
            private BufferLookup<ChildEntity> m_LookupChildEntity;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Aspect>()
                    .WithAspect<Logic.Aspect>()
                    .Build();
                //m_Query.SetChangedVersionFilter(ComponentType.ReadOnly<Logic>());
                state.RequireForUpdate(m_Query);
                
                m_LookupTeams = state.GetComponentLookup<Team>(true);
                m_LookupLogicAspect = new Logic.Aspect.Lookup(ref state);
                m_LookupMapTransform = state.GetComponentLookup<Map.Move>(true);
                m_LookupChildEntity = state.GetBufferLookup<ChildEntity>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<GameLogicCommandBufferSystem.Singleton>();
                m_LookupLogicAspect.Update(ref state);
                m_LookupMapTransform.Update(ref state);
                m_LookupChildEntity.Update(ref state);
                m_LookupTeams.Update(ref state);
                state.Dependency = new SystemJob()
                {
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    Delta = SystemAPI.Time.DeltaTime,
                    LookupTeams = m_LookupTeams,

                    LookupLogicAspect = m_LookupLogicAspect,
                    LookupMapTransform = m_LookupMapTransform,
                    LookupChildEntity = m_LookupChildEntity,
                    
                }.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                public EntityCommandBuffer.ParallelWriter Writer;
                [ReadOnly] public ComponentLookup<Team> LookupTeams;

                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Logic.Aspect.Lookup LookupLogicAspect;
                
                [ReadOnly] public ComponentLookup<Map.Move> LookupMapTransform;
                [ReadOnly] public BufferLookup<ChildEntity> LookupChildEntity;

                void Execute([EntityIndexInQuery] int idx, Aspect unit)//Logic.Aspect logic, 
                {
                    /* logic
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

                    var context = new Context(idx, ref LookupLogicAspect, ref unit,
                        ref LookupMapTransform, ref LookupChildEntity, ref Writer);
                    
                    LookupLogicAspect[logic.Self].ExecuteBeforeAction(context);

                    if (logic.IsCurrentAction(Move.Action.FindPath))
                    {
                        //logic.
                    }
                    
                    if (logic.IsCurrentAction(Global.Action.Destroy))
                    {
                        logic.SetWorldState(Global.State.Dead, true);
                    }

                    LookupLogicAspect[logic.Self].ExecuteAfterAction(context);
                    */
                }
            }
        }
    }
}