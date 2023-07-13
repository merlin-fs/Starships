using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Logics
{
    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(Logic.System))]
    public partial class GamePartLogicSystemGroup : ComponentSystemGroup { }


    public partial struct Logic
    {
        [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
        public partial class System : SystemBase
        {
            EntityQuery m_Query;
            protected override void OnCreate()
            {
                PlanFinder.Init();
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Logic.Aspect>()
                    .Build();
                
                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<WorldState>());
                RequireForUpdate(m_Query);
            }

            protected override void OnDestroy()
            {
                PlanFinder.Dispose();
                base.OnDestroy();
            }

            partial struct StateMachineJob : IJobEntity
            {
                [NativeSetThreadIndex]
                int m_ThreadIndex;

                public void Execute(Logic.Aspect logic)
                {
                    if (!logic.IsValid) return;
                    if (!logic.IsActive) return;
                    
                    if (logic.IsWaitNewGoal || logic.IsWaitChangeWorld)
                        return;

                    logic.CheckCurrentAction();

                    if (logic.IsWork)
                        return;

                    if (!logic.HasPlan)
                    {
                        if (logic.GetNextGoal(out Goal goal))
                        {
                            var plan = PlanFinder.Execute(m_ThreadIndex, logic, goal, Allocator.TempJob);
                            if (plan.IsCreated && plan.Length > 0)
                            {
                                logic.SetPlan(plan);
                                plan.Dispose();
                            }
                            else
                            {
                                logic.SetAction(LogicHandle.Null);
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

                    if (!logic.IsAction || (logic.IsAction && logic.IsActionSuccess()))
                    {
                        var next = logic.GetNextState();
                        logic.SetAction(next);
                    }
                    else
                    {
                        logic.SetFailed();
                    }
                }
            }

            protected override void OnUpdate()
            {
                var selectJob = new StateMachineJob()
                {
                };
                Dependency = selectJob.ScheduleParallel(m_Query, Dependency);
            }
        }
    }
}
