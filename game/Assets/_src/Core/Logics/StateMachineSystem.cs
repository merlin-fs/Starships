using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;

namespace Game.Model.Logics
{
    using System.Threading;

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

                void Execute(ref LogicAspect logic)
                {
                    if (!logic.IsValid) return;

                    if (!logic.IsWork)
                    {
                        //UnityEngine.Debug.Log($"{logic.Self} [Logic] work - {logic.IsWork}, {logic.CurrentAction}");
                        if (!logic.HasPlan)
                        {
                            var goal = logic.Def.GetGoal();
                            var plan = PlanFinder.Execute(m_ThreadIndex, logic, goal, Allocator.TempJob);
                            if (plan.IsCreated && plan.Length > 0)
                            {
                                UnityEngine.Debug.Log($"{logic.Self} [Logic] new plan - {string.Join(", ", plan.ToArray().Select(i => $"{i}"))}");
                                logic.SetPlan(plan);
                                plan.Dispose();
                            }
                            else
                            {
                                logic.SetFailed();
                                return;
                            }
                        }

                        if (!logic.IsAction() || (logic.IsAction() && logic.IsActionSuccess()))
                        {
                            var next = logic.GetNextState();
                            UnityEngine.Debug.Log($"{logic.Self} [Logic] new action - {next}");
                            logic.SetAction(next);
                        }
                        else
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
