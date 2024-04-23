using System;
using System.Linq;

using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Game.Core;

using Unity.Jobs;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
        public partial struct System : ISystem
        {
            private EntityQuery m_Query;
            private NativeQueue<Data> m_Queue;

            private static int m_UpdateCount;
            public static int UpdateCount => m_UpdateCount; 
            
            private struct Data : IComponentData
            {
                public Cmd Command;

                public enum Cmd
                {
                    Activate,
                    Deactivate,
                }
            }

            public void OnCreate(ref SystemState state)
            {
                m_UpdateCount = 0;
                PlanFinder.Initialize();
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<InternalAspect>()
                    .Build();

                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<WorldState>());

                m_Queue = new NativeQueue<Data>(Allocator.Persistent);
                state.RequireForUpdate(m_Query);
            }

            public void Activate(bool value)
            {
                m_Queue.Enqueue(new Data {Command = value ? Data.Cmd.Activate : Data.Cmd.Deactivate});
            }

            public void OnDestroy(ref SystemState state)
            {
                m_Queue.Dispose();
                PlanFinder.Dispose();
            }

            private void CheckCommands(ref SystemState state)
            {
                if (m_Queue.Count <= 0) return;

                state.Dependency = new ActivateJob 
                {
                    Active = m_Queue.Dequeue().Command == Data.Cmd.Activate,
                }.ScheduleParallel(m_Query, state.Dependency);
            }

            private partial struct ActivateJob : IJobEntity
            {
                public bool Active;

                private void Execute([EntityIndexInQuery] int idx, InternalAspect logic, Entity entity)
                {
                    logic.SetActive(Active);
                }
            }


            public void OnUpdate(ref SystemState state)
            {
                m_UpdateCount++;
                CheckCommands(ref state);
                
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

                state.Dependency = new StateMachineJob() {
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                }.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct StateMachineJob : IJobEntity
            {
                [NativeSetThreadIndex] int m_ThreadIndex;
                public EntityCommandBuffer.ParallelWriter Writer;

                
                private void Execute([EntityIndexInQuery] int idx, InternalAspect logic, Entity entity)
                {
                    if (!logic.IsValid) return;
                    if (!logic.IsActive) return;

                    if ( /*logic.IsWaitNewGoal ||*/ logic.IsWaitChangeWorld)
                        return;

                    logic.CheckCurrentAction();

                    if (logic.IsWork)
                        return;

                    if (!logic.HasPlan && !logic.IsEvent)
                    {
                        if (logic.GetNextGoal(out Goal goal))
                        {
                            if (logic.HasWorldState(goal.State, goal.Value)) return;

                            var plan = PlanFinder.Execute(m_ThreadIndex, logic, goal, logic.Def, Allocator.TempJob);
                            if (plan.IsCreated && plan.Length > 0)
                            {
                                logic.SetPlan(plan);
                                plan.Dispose();
                            }
                            else
                            {
                                logic.SetAction(LogicActionHandle.Null);
                                logic.SetWaitChangeWorld();
                                return;
                            }
                        }
                        else
                        {
                            logic.SetWaitNewGoal();
                            return;
                        }
                    }

                    if (logic.IsEvent)
                    {
                        logic.ResetEvent();
                    }
                    else if (!logic.IsAction || (logic.IsAction && logic.IsActionSuccess()))
                    {
                        var next = logic.GetNextState();
                        logic.SetAction(next.Value);
                    }
                    else
                    {
                        logic.SetFailed();
                    }
                }
            }
        }


        [UpdateInGroup(typeof(GameEndSystemGroup), OrderFirst = true)]
        public partial struct EndLogicSystem : ISystem
        {
            private EntityQuery m_Query;
            private EntityQuery m_QueryInit;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Aspect>()
                    .Build();

                m_QueryInit = SystemAPI.QueryBuilder()
                    .WithAspect<Aspect>()
                    .WithAll<InitTag>()
                    .Build();
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

                state.Dependency =
                    new ChangeInitDoneJob {
                        Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    }.ScheduleParallel(m_QueryInit, state.Dependency);

                state.Dependency = new ChangeWorldDoneJob { }.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct ChangeInitDoneJob : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter Writer;

                private void Execute([EntityIndexInQuery] int idx, Entity entity, InternalAspect logic)
                {
                    if (!logic.IsCurrentAction(logic.Def.InitializeAction)) return;

                    logic.SetFailed();
                    Writer.RemoveComponent<InitTag>(idx, entity);
                }
            }

            partial struct ChangeWorldDoneJob : IJobEntity
            {
                private void Execute(InternalAspect logic)
                {
                    if (!logic.IsChangedWorld) return;
                    logic.ChangedWorldClear();
                }
            }
        }
    }
}
