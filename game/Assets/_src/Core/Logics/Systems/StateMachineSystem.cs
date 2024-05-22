using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Game.Core;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
        public partial struct System : ISystem
        {
            private EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                PlanFinder.Initialize();
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<InternalAspect>()
                    .WithAll<ChangeTag>()
                    .Build();

                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<ChangeTag>());
                state.RequireForUpdate(m_Query);
            }

            public void OnDestroy(ref SystemState state)
            {
                PlanFinder.Dispose();
            }

            public void OnUpdate(ref SystemState state)
            {
                if (m_Query.IsEmpty) return;
                
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

                    if ( /*logic.IsWaitNewGoal ||*/ logic.IsWaitChangeWorld)
                        return;

                    if (logic.IsActionSuccess())
                    {
                        SetNext();
                    }
                    
                    logic.CheckCurrentAction();

                    if (logic.IsWork)
                        return;

                    if (!logic.HasPlan)
                    {
                        if (logic.GetNextGoal(out Goal goal))
                        {
                            if (logic.HasWorldState(goal.State, goal.Value)) return;

                            var plan = PlanFinder.Execute(m_ThreadIndex, logic, goal, logic.Def, Allocator.TempJob);
                            if (plan.IsCreated && plan.Length > 0)
                            {
                                logic.SetPlan(plan);
                                plan.Dispose();
                                SetNext();
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

                    void SetNext()
                    {
                        var next = logic.GetNextState();
                        logic.SetAction(next.Value);
                    }
                }
            }
        }
    }
}
