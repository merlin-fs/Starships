using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Properties;

using static UnityEngine.Rendering.DebugUI;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public readonly partial struct Aspect : IAspect
        {
            private readonly Entity m_Self;
            private readonly RefRO<Root> m_Root;
            private readonly RefRW<Logic> m_Logic;
            private readonly DynamicBuffer<LogicHandle> m_Plan;
            private readonly DynamicBuffer<WorldState> m_WorldStates;
            private readonly DynamicBuffer<Goal> m_Goals;
            public Entity Self => m_Self;
            public Entity Root => m_Root.ValueRO.Value;
            public LogicDef Def => m_Logic.ValueRO.Def;
            [CreateProperty]
            public string CurrentAction => m_Logic.ValueRO.Action.ToString();
            [CreateProperty]
            public bool IsWork => m_Logic.ValueRO.Work;
            public bool IsWaitNewGoal => m_Logic.ValueRO.WaitNewGoal;
            public bool IsWaitChangeWorld => m_Logic.ValueRO.WaitChangeWorld;
            public bool IsValid => m_Logic.ValueRO.Def.IsValid;
            public bool HasPlan => m_Plan.Length > 0;
            public bool IsAction => m_Logic.ValueRO.Action != LogicHandle.Null;


            public void CheckCurrentAction()
            {
                //UnityEngine.Debug.Log($"{Self} [Logic] check: work-{IsWork}, action-{m_Logic.ValueRO.Action}");
                if (IsWork && IsAction)
                {
                    if (Def.TryGetAction(m_Logic.ValueRO.Action, out GoapAction action) && !action.CanTransition(m_WorldStates, Def))
                        SetFailed();
                }
            }

            public bool IsCurrentAction(Enum action)
            {
                return m_Logic.ValueRO.Action == LogicHandle.FromEnumTest(action);
            }

            public bool IsCurrentAction(LogicHandle action)
            {
                return m_Logic.ValueRO.Action == action;
            }

            public void SetWorldState(Enum worldState, bool value)
            {
                var state = LogicHandle.FromEnum(worldState);
                var index = m_Logic.ValueRO.Def.StateMapping[state].Index;
                m_WorldStates.ElementAt(index).Value = value;
                //UnityEngine.Debug.Log($"{Self} [Logic] change world: {state} - {value}");
                if (Def.TryGetAction(m_Logic.ValueRO.Action, out GoapAction action) && action.GetGoalTools().LeadsToGoal(state))
                {
                    //UnityEngine.Debug.Log($"{Self} [Logic] {CurrentAction} - done");
                    m_Logic.ValueRW.Work = false;
                }
                m_Logic.ValueRW.WaitChangeWorld = false;
            }

            public bool HasWorldState(Enum worldState, bool value)
            {
                var index = m_Logic.ValueRO.Def.StateMapping[LogicHandle.FromEnumTest(worldState)].Index;
                return m_WorldStates[index].Value == value;
            }

            public bool HasWorldState(LogicHandle worldState, bool value)
            {
                var index = m_Logic.ValueRO.Def.StateMapping[worldState].Index;
                return m_WorldStates[index].Value == value;
            }

            public void SetGoals()
            {
                //TODO: нужно доделать. установка новых целей.
            }

            public bool GetNextGoal(out Goal goal)
            {
                var result = (m_Goals.Length > 0);
                goal = default;
                if (result)
                {
                    goal = m_Goals[^1];
                    if (!goal.Repeat)
                        m_Goals.RemoveAt(m_Goals.Length - 1);
                }
                return result;
            }

            public void SetPlan(NativeArray<LogicHandle> plan)
            {
                //UnityEngine.Debug.Log($"{Self} [Logic] new plan - {string.Join(", ", plan.ToArray().Select(i => $"{i}"))}");
                m_Plan.CopyFrom(plan);
            }

            public bool IsActionSuccess()
            {
                return Def.TryGetAction(m_Logic.ValueRO.Action, out GoapAction action) && action.IsSuccess(m_WorldStates, Def);
            }

            public void SetFailed()
            {
                //UnityEngine.Debug.Log($"{Self} [Logic] {CurrentAction} - Failed");
                m_Logic.ValueRW.Action = LogicHandle.Null;
                m_Logic.ValueRW.Work = false;
            }

            public void SetWaitChangeWorld()
            {
                UnityEngine.Debug.Log($"{Self} [Logic] no plan. Wait change world");
                m_Logic.ValueRW.Action = LogicHandle.Null;
                m_Logic.ValueRW.WaitChangeWorld = true;
            }

            public void SetWaitNewGoal()
            {
                UnityEngine.Debug.Log($"{Self} [Logic] LogicFinish - no goals");
                m_Logic.ValueRW.Action = LogicHandle.Null;
                m_Logic.ValueRW.WaitNewGoal = true;
            }

            public void Interrupt()
            {
                m_Logic.ValueRW.Action = LogicHandle.Null;
                m_Logic.ValueRW.Work = false;
                //UnityEngine.Debug.Log($"{Self} [Logic] Interrupt");
            }

            public void SetEvent(Enum value)
            {
                SetEvent(LogicHandle.FromEnum(value));
            }

            public void SetEvent(LogicHandle value)
            {
                if (Def.TryGetAction(value, out GoapAction action) && action.CanTransition(m_WorldStates, Def))
                {
                    m_Logic.ValueRW.Action = value;
                    m_Logic.ValueRW.Work = false;
                    UnityEngine.Debug.Log($"{Self} [Logic] new event \"{m_Logic.ValueRO.Action}\"");
                }
            }

            public void SetAction(LogicHandle value)
            {
                m_Logic.ValueRW.Action = value;
                m_Logic.ValueRW.Work = value != LogicHandle.Null;
                if (Def.TryGetAction(value, out GoapAction action) && !action.CanTransition(m_WorldStates, Def))
                {
                    SetFailed();
                }
                //UnityEngine.Debug.Log($"{Self} [Logic] new action \"{m_Logic.ValueRO.Action}\"");
            }

            public LogicHandle GetNextState()
            {
                var next = m_Plan.Length > 0
                    ? m_Plan[^1]
                    : LogicHandle.Null;
                if (m_Plan.Length > 0)
                    m_Plan.RemoveAt(m_Plan.Length - 1);
                return next;
            }
        }
    }
}