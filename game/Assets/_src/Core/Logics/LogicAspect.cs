using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Properties;
using Game.Core;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public readonly partial struct Aspect : IAspect
        {
            #region debug info
            [CreateProperty] private bool Valid => IsValid;
            [CreateProperty] private bool Active => IsActive;
            [CreateProperty] private bool Work => IsWork;
            //[CreateProperty] private bool WaitNewGoal => IsWaitNewGoal;
            [CreateProperty] private bool WaitChangeWorld => IsWaitChangeWorld;
            [CreateProperty] public string Action => m_Logic.ValueRO.m_Action.ToString();
            [CreateProperty] private List<Plan> Plan => m_Plan.AsNativeArray().ToArray().ToList();
            [CreateProperty] private List<Goal> Goal => m_Goals.AsNativeArray().ToArray().ToList();
            private struct WorldStatesDebug
            {
                public string ID;
                public bool Value;
            }
            [CreateProperty] private List<WorldStatesDebug> WorldStates => BuildList();
            
            private List<WorldStatesDebug> BuildList()
            {
                var map = m_WorldStates;
                return m_Logic.ValueRO.Def.StateMapping.Select(pair => new WorldStatesDebug{ID = pair.Key.ToString(), Value = map[pair.Value.Index].Value})
                    .ToList();
            }
            #endregion
            private readonly Entity m_Self;
            private readonly RefRO<Root> m_Root;
            private readonly RefRW<Logic> m_Logic;
            private readonly DynamicBuffer<Plan> m_Plan;
            private readonly DynamicBuffer<WorldState> m_WorldStates;
            private readonly DynamicBuffer<Goal> m_Goals;
            public Entity Self => m_Self;
            public string SelfName => World.DefaultGameObjectInjectionWorld.EntityManager.GetName(m_Self);
            public Entity Root => m_Root.ValueRO.Value;
            public LogicDef Def => m_Logic.ValueRO.Def;
            public bool IsWork => m_Logic.ValueRO.m_Work;
            //public bool IsWaitNewGoal => m_Logic.ValueRO.m_WaitNewGoal;
            public bool IsValid => m_Logic.ValueRO.Def.IsValid;
            public bool IsActive => m_Logic.ValueRO.m_Active;
            public bool IsAction => m_Logic.ValueRO.m_Action != EnumHandle.Null;

            private bool IsChangedWorld => m_Logic.ValueRO.m_ChangedWorld != GoalHandle.Null;
            private bool IsWaitChangeWorld => m_Logic.ValueRO.m_WaitChangeWorld;
            private bool HasPlan => m_Plan.Length > 0;

            public bool IsCurrentAction<T>(T action)
                where T: struct, IConvertible
            {
                return m_Logic.ValueRO.IsCurrentAction(EnumHandle.FromEnum(action));
            }

            public bool IsCurrentAction(EnumHandle action)
            {
                return m_Logic.ValueRO.IsCurrentAction(action);
            }

            public void SetWorldState<T>(T worldState, bool value)
                where T: struct, IConvertible
            {
                var state = EnumHandle.FromEnum(worldState);
                var index = m_Logic.ValueRO.Def.StateMapping[state].Index;
                if (m_WorldStates.ElementAt(index).Value != value)
                {
                    m_WorldStates.ElementAt(index).Value = value;
#if LOGIC_DEBUG                    
                    UnityEngine.Debug.Log($"{Self},{SelfName} [Logic] change world: {state} - {value}");
#endif
                }
                if (Def.TryGetAction(m_Logic.ValueRO.m_Action, out GoapAction action) && action.GetGoalTools().LeadsToGoal(state))
                {
#if LOGIC_DEBUG                    
                    UnityEngine.Debug.Log($"{Self} [Logic] {Action} - done");
#endif
                    m_Logic.ValueRW.m_Work = false;
                }
                m_Logic.ValueRW.m_WaitChangeWorld = false;
                m_Logic.ValueRW.m_ChangedWorld = GoalHandle.FromHandle(state, value);
            }
            
            public bool HasWorldState<T>(T worldState, bool value)
                where T: struct, IConvertible
            {
                var index = m_Logic.ValueRO.Def.StateMapping[EnumHandle.FromEnum(worldState)].Index;
                return m_WorldStates[index].Value == value;
            }

            public bool HasWorldState(EnumHandle worldState, bool value)
            {
                var index = m_Logic.ValueRO.Def.StateMapping[worldState].Index;
                return m_WorldStates[index].Value == value;
            }

            //TODO: Доделать на стороне StateMachine
            public void SetEvent<T>(T value)
                where T: struct, IConvertible
            {
                SetEvent(EnumHandle.FromEnum(value));
            }
            
            public void SetEvent(EnumHandle value)
            {
                if (Def.TryGetAction(value, out GoapAction action) && action.CanTransition(m_WorldStates, Def))
                {
                    m_Logic.ValueRW.m_Action = value;
                    m_Logic.ValueRW.m_Work = false;
                    m_Logic.ValueRW.m_Event = true;
#if LOGIC_DEBUG                
                    UnityEngine.Debug.Log($"{Self},{SelfName} [Logic] new event \"{Action}\"");
#endif
                }
            }
            
            public void SetGoals()
            {
                //TODO: нужно доделать. установка новых целей.
            }

            private void SetActive(bool value)
            {
                m_Logic.ValueRW.m_Active = value;
            }

            private void CheckCurrentAction()
            {
                if (!IsWork || !IsAction) return;
                
                if (Def.TryGetAction(m_Logic.ValueRO.m_Action, out GoapAction action) && !action.CanTransition(m_WorldStates, Def))
                    SetFailed();
            }

            private void ExecuteTriggersState(ref LogicContext context)
            {
                var state = m_Logic.ValueRO.m_ChangedWorld;
                m_Logic.ValueRW.m_ChangedWorld = GoalHandle.Null;
                Def.ExecuteTriggersState(ref context, state);
            }

            private void ExecuteTriggersAction(ref LogicContext context)
            {
                Def.ExecuteTriggersAction(ref context, m_Logic.ValueRO.m_Action);
            }

            private bool GetNextGoal(out Goal goal)
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

            private void SetPlan(NativeArray<Plan> plan)
            {
#if LOGIC_DEBUG
                UnityEngine.Debug.Log($"{Self} [Logic] new plan - {string.Join(" > ", plan.ToArray().Reverse().Select(i => $"{i.Value.ToString()}"))}");
#endif
                m_Plan.CopyFrom(plan);
            }

            private bool IsActionSuccess()
            {
                return Def.TryGetAction(m_Logic.ValueRO.m_Action, out GoapAction action) && action.IsSuccess(m_WorldStates, Def);
            }

            private void SetFailed()
            {
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self},{SelfName} [Logic] {Action} - Failed");
#endif
                m_Logic.ValueRW.m_Action = EnumHandle.Null;
                m_Logic.ValueRW.m_Work = false;
            }

            private void SetWaitChangeWorld()
            {
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self},{SelfName} [Logic] no plan. Wait change world");
#endif
                m_Logic.ValueRW.m_WaitChangeWorld = true;
            }

            private void SetWaitNewGoal()
            {
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self},{SelfName} [Logic] LogicFinish - no goals");
#endif
                m_Logic.ValueRW.m_Action = EnumHandle.Null;
                m_Logic.ValueRW.m_WaitNewGoal = true;
            }

            private void SetAction(EnumHandle value)
            {
                m_Logic.ValueRW.m_Action = value;
                m_Logic.ValueRW.m_Work = value != EnumHandle.Null;
                if (Def.TryGetAction(value, out GoapAction action) && !action.CanTransition(m_WorldStates, Def))
                {
                    SetFailed();
                }
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self},{SelfName} [Logic] new action \"{Action}\"");
#endif
            }

            private bool IsEvent => m_Logic.ValueRO.m_Event;
            private void ResetEvent() => m_Logic.ValueRW.m_Event = false;

            private Plan GetNextState()
            {
                var next = m_Plan.Length > 0
                    ? m_Plan[^1]
                    : EnumHandle.Null;
                if (m_Plan.Length > 0)
                    m_Plan.RemoveAt(m_Plan.Length - 1);
                return next;
            }
        }
    }
}