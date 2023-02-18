using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Logics
{
    using Stats;

    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderLast = true)]
    public class GamePartLogicSystemGroup : ComponentSystemGroup { }

    public partial struct Logic
    {
        //[UpdateInGroup(typeof(GameLogicSystemGroup), OrderFirst = true)]
        [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
        public partial class StateMachineSystem : SystemBase
        {
            EntityQuery m_Query;
            protected override void OnCreate()
            {
                PlanFinder.Init();
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Logic>()
                    .WithAll<WorldState>()
                    .WithNone<DeadTag>()
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

                public void Execute(ref LogicAspect logic)
                {
                    if (!logic.IsValid) return;

                    logic.CheckCurrentAction();

                    if (logic.IsWork || logic.IsWaitNewGoal || logic.IsWaitChangeWorld)
                        return;

                    if (!logic.HasPlan)
                    {
                        if (logic.GetNextGoal(out Goal goal))
                        {
                            using var plan = PlanFinder.Execute(m_ThreadIndex, logic, goal, Allocator.TempJob);
                            if (plan.IsCreated && plan.Length > 0)
                            {
                                logic.SetPlan(plan);
                            }
                            else
                            {
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
