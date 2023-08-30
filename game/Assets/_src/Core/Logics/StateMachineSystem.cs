using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Game.Core;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public readonly partial struct Aspect
        {

            [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
            public partial struct System : ISystem
            {
                private EntityQuery m_Query;
                private Aspect.Lookup m_LookupLogicAspect;
                private BufferLookup<ChildEntity> m_LookupChildEntity;

                private NativeQueue<Data> m_Queue;

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
                    PlanFinder.Init();
                    m_Query = SystemAPI.QueryBuilder()
                        .WithAspect<Aspect>()
                        .Build();

                    m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<WorldState>());

                    m_LookupLogicAspect = new Aspect.Lookup(ref state);
                    m_LookupChildEntity = state.GetBufferLookup<ChildEntity>(true);
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
                        LookupLogicAspect = m_LookupLogicAspect,
                        Active = m_Queue.Dequeue().Command == Data.Cmd.Activate,
                    }.ScheduleParallel(m_Query, state.Dependency);
                }

                partial struct ActivateJob : IJobEntity
                {
                    [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                    public Aspect.Lookup LookupLogicAspect;

                    public bool Active;

                    private void Execute([EntityIndexInQuery] int idx, Entity entity)
                    {
                        var logic = LookupLogicAspect[entity];
                        logic.SetActive(Active);
                    }
                }


                public void OnUpdate(ref SystemState state)
                {
                    m_LookupLogicAspect.Update(ref state);
                    CheckCommands(ref state);
                    m_LookupChildEntity.Update(ref state);
                    var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

                    state.Dependency = new StateMachineJob() {
                        Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                        LookupLogicAspect = m_LookupLogicAspect,
                        LookupChildEntity = m_LookupChildEntity,
                    }.ScheduleParallel(m_Query, state.Dependency);
                }

                partial struct StateMachineJob : IJobEntity
                {
                    [NativeSetThreadIndex] int m_ThreadIndex;
                    public EntityCommandBuffer.ParallelWriter Writer;

                    [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                    public Aspect.Lookup LookupLogicAspect;

                    [ReadOnly] public BufferLookup<ChildEntity> LookupChildEntity;

                    private void Execute([EntityIndexInQuery] int idx, Entity entity)
                    {
                        var logic = LookupLogicAspect[entity];
                        if (!logic.IsValid) return;
                        if (!logic.IsActive) return;

                        if ( /*logic.IsWaitNewGoal ||*/ logic.IsWaitChangeWorld)
                            return;

                        if (logic.IsChangedWorld)
                        {
                            var context = new LogicContext(idx, ref logic, ref LookupLogicAspect, ref LookupChildEntity, ref Writer);
                            logic.ExecuteTriggersState(ref context);
                        }

                        logic.CheckCurrentAction();

                        if (logic.IsWork)
                            return;

                        if (!logic.HasPlan && !logic.IsEvent)
                        {
                            if (logic.GetNextGoal(out Goal goal))
                            {
                                if (logic.HasWorldState(goal.State, goal.Value)) return;

                                var plan = PlanFinder.Execute(m_ThreadIndex, logic, goal, Allocator.TempJob);
                                if (plan.IsCreated && plan.Length > 0)
                                {
                                    logic.SetPlan(plan);
                                    plan.Dispose();
                                }
                                else
                                {
                                    logic.SetAction(EnumHandle.Null);
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
                            var context = new LogicContext(idx, ref logic, ref LookupLogicAspect, ref LookupChildEntity, ref Writer);
                            logic.ExecuteTriggersAction(ref context);
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

                public void OnCreate(ref SystemState state)
                {
                    m_Query = SystemAPI.QueryBuilder()
                        .WithAspect<Aspect>()
                        .WithAll<InitTag>()
                        .Build();
                    state.RequireForUpdate(m_Query);
                }

                public void OnUpdate(ref SystemState state)
                {
                    var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                    var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
                    state.Dependency = new SystemJob
                    {
                        Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    }.ScheduleParallel(m_Query, state.Dependency);
                }

                partial struct SystemJob : IJobEntity
                {
                    public EntityCommandBuffer.ParallelWriter Writer;

                    private void Execute([EntityIndexInQuery] int idx, Aspect logic)
                    {
                        if (!logic.IsCurrentAction(logic.Def.InitializeAction)) return;

                        logic.SetFailed();
                        Writer.RemoveComponent<InitTag>(idx, logic.Self);
                    }
                }
            }
        }
    }
}
