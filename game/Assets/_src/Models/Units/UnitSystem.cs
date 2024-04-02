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

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Aspect>()
                    .WithAspect<Logic.Aspect>()
                    .Build();
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
                state.RequireForUpdate(m_Query);
                
                m_LookupTeams = state.GetComponentLookup<Team>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<GameLogicCommandBufferSystem.Singleton>();
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

                void Execute([EntityIndexInQuery] int idx, Logic.Aspect logic, Aspect unit)
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

                    if (logic.IsCurrentAction(Move.Action.FindPath))
                    {
                        //logic.
                    }
                    
                    if (logic.IsCurrentAction(Global.Action.Destroy))
                    {
                        logic.SetWorldState(Global.State.Dead, true);
                    }
                }
            }
        }
        
        [UpdateInGroup(typeof(GameLogicBeforeActionSystemGroup))]
        public partial struct BeforeActionSystem : ISystem
        {
            private EntityQuery m_Query;
            private Logic.Aspect.Lookup m_LookupLogicAspect;
            private ComponentLookup<Map.Transform> m_LookupMapTransform;
            private BufferLookup<ChildEntity> m_LookupChildEntity;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Aspect>()
                    .WithAspect<Logic.Aspect>()
                    .Build();
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Unit>());
                state.RequireForUpdate(m_Query);
                
                m_LookupLogicAspect = new Logic.Aspect.Lookup(ref state);
                m_LookupMapTransform = state.GetComponentLookup<Map.Transform>(true);
                m_LookupChildEntity = state.GetBufferLookup<ChildEntity>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupLogicAspect.Update(ref state);
                m_LookupMapTransform.Update(ref state);
                m_LookupChildEntity.Update(ref state);
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                state.Dependency = new SystemJob()
                {
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    Delta = SystemAPI.Time.DeltaTime,
                    LookupLogicAspect = m_LookupLogicAspect,
                    LookupMapTransform = m_LookupMapTransform,
                    LookupChildEntity = m_LookupChildEntity,
                }.ScheduleParallel(m_Query, state.Dependency);
            }
            
            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                public EntityCommandBuffer.ParallelWriter Writer;
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Logic.Aspect.Lookup LookupLogicAspect;
                [ReadOnly] public ComponentLookup<Map.Transform> LookupMapTransform;
                [ReadOnly] public BufferLookup<ChildEntity> LookupChildEntity;

                void Execute([EntityIndexInQuery] int idx, Aspect aspect)
                {
                    LookupLogicAspect[aspect.Self]
                        .ExecuteBeforeAction(new Context(idx, ref LookupLogicAspect, ref aspect,
                            ref LookupMapTransform, ref LookupChildEntity, ref Writer));
                }
            }
        }
        [UpdateInGroup(typeof(GameLogicAfterActionSystemGroup))]
        public partial struct AfterActionSystem : ISystem
        {
            private EntityQuery m_Query;
            private Logic.Aspect.Lookup m_LookupLogicAspect;
            private ComponentLookup<Map.Transform> m_LookupMapTransform;
            private BufferLookup<ChildEntity> m_LookupChildEntity;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Aspect>()
                    .WithAspect<Logic.Aspect>()
                    .Build();
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Unit>());
                state.RequireForUpdate(m_Query);
                
                m_LookupLogicAspect = new Logic.Aspect.Lookup(ref state);
                m_LookupMapTransform = state.GetComponentLookup<Map.Transform>(true);
                m_LookupChildEntity = state.GetBufferLookup<ChildEntity>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupLogicAspect.Update(ref state);
                m_LookupMapTransform.Update(ref state);
                m_LookupChildEntity.Update(ref state);
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                state.Dependency = new SystemJob()
                {
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    Delta = SystemAPI.Time.DeltaTime,
                    LookupLogicAspect = m_LookupLogicAspect,
                    LookupMapTransform = m_LookupMapTransform,
                    LookupChildEntity = m_LookupChildEntity,
                }.ScheduleParallel(m_Query, state.Dependency);
            }
            
            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                public EntityCommandBuffer.ParallelWriter Writer;
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Logic.Aspect.Lookup LookupLogicAspect;
                [ReadOnly] public ComponentLookup<Map.Transform> LookupMapTransform;
                [ReadOnly] public BufferLookup<ChildEntity> LookupChildEntity;

                void Execute([EntityIndexInQuery] int idx, Aspect aspect)
                {
                    LookupLogicAspect[aspect.Self]
                        .ExecuteAfterChangeState(new Context(idx, ref LookupLogicAspect, ref aspect, 
                            ref LookupMapTransform, ref LookupChildEntity, ref Writer));
                }
            }
        }
    }
}