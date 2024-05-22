using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Properties;
using Game.Core;

using Reflex.Attributes;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public interface IWorldState
        {
            bool HasWorldState(EnumHandle worldState, bool value);
        }
        
        private readonly partial struct InternalAspect : IAspect, IWorldState
        {
            private readonly Entity m_Self;
            private readonly RefRO<Root> m_Root;
            private readonly RefRW<Logic> m_Logic;
            private readonly DynamicBuffer<Plan> m_Plan;
            [ReadOnly]
            private readonly DynamicBuffer<WorldState> m_WorldStates;
            private readonly DynamicBuffer<Goal> m_Goals;
            public LogicDef Def => m_Logic.ValueRO.Def;
            public bool IsValid => m_Logic.ValueRO.Def.IsValid;
            public bool IsAction => m_Logic.ValueRO.m_Action != LogicActionHandle.Null;
            public bool IsWork => m_Logic.ValueRO.m_Work;
            public bool IsWaitChangeWorld => m_Logic.ValueRO.m_WaitChangeWorld;
            public bool HasPlan => m_Plan.Length > 0;

            public void SetGoals()
            {
                //TODO: нужно доделать. установка новых целей.
            }

            public void CheckCurrentAction()
            {
                if (!IsWork || !IsAction) return;
                
                if (Def.TryGetAction(m_Logic.ValueRO.m_Action, out GoapAction action) && 
                    !action.CanTransition(m_WorldStates, Def))
                    SetFailed();
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

            public void SetPlan(NativeArray<Plan> plan)
            {
#if LOGIC_DEBUG
                UnityEngine.Debug.Log($"{Self}({SelfName}) {System.UpdateCount} [Logic] new plan - {string.Join(" > ", plan.ToArray().Reverse().Select(i => $"{i.Value.ToString()}"))}");
#endif
                m_Plan.CopyFrom(plan);
            }

            public bool IsActionSuccess()
            {
                return Def.TryGetAction(m_Logic.ValueRO.m_Action, out GoapAction action) 
                       && action.IsSuccess(m_WorldStates, Def);
            }

            public void SetFailed()
            {
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self}({SelfName}) {System.UpdateCount} [Logic] {Action} - Failed");
#endif
                m_Logic.ValueRW.m_Action = LogicActionHandle.Null;
                m_Logic.ValueRW.m_Work = false;
                m_Plan.Clear();
            }

            public void SetWaitChangeWorld()
            {
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self}({SelfName}) {System.UpdateCount} [Logic] no plan. Wait change world");
#endif
                m_Logic.ValueRW.m_WaitChangeWorld = true;
            }

            public bool HasWorldState(EnumHandle worldState, bool value)
            {
                var index = m_Logic.ValueRO.Def.StateMapping[worldState].Index;
                return m_WorldStates[index].Value == value;
            }

            public void SetWaitNewGoal()
            {
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self}({SelfName}) {System.UpdateCount} [Logic] LogicFinish - no goals");
#endif
                m_Logic.ValueRW.m_Action = LogicActionHandle.Null;
                m_Logic.ValueRW.m_WaitNewGoal = true;
            }

            public void SetAction(LogicActionHandle value)
            {
                m_Logic.ValueRW.m_Action = value;
                m_Logic.ValueRW.m_Work = value != LogicActionHandle.Null;
                if (Def.TryGetAction(value, out GoapAction action) && !action.CanTransition(m_WorldStates, Def))
                {
                    SetFailed();
                }
#if LOGIC_DEBUG                
                UnityEngine.Debug.Log($"{Self}({SelfName}) {System.UpdateCount} [Logic] new action \"{Action}\"");
#endif
            }

            public Plan GetNextState()
            {
                var next = m_Plan.Length > 0
                    ? m_Plan[^1]
                    : LogicActionHandle.Null;
                if (m_Plan.Length > 0)
                    m_Plan.RemoveAt(m_Plan.Length - 1);
                return next;
            }
        }

        public readonly partial struct Aspect : IAspect, IWorldState
        {
            private readonly Entity m_Self;
            private readonly RefRO<Root> m_Root;
            private readonly RefRW<Logic> m_Logic;
            [ReadOnly]
            private readonly DynamicBuffer<WorldState> m_WorldStates;
            public Entity Self => m_Self;
            public Entity Root => m_Root.ValueRO.Value;
            public LogicDef Def => m_Logic.ValueRO.Def;
            private bool IsAction => m_Logic.ValueRO.m_Action != LogicActionHandle.Null;
            public LogicActionHandle Action => m_Logic.ValueRO.m_Action;
            
            public void SetWorldState<T>(T worldState, bool value)
                where T : struct, IConvertible
            {
                var state = EnumHandle.FromEnum(worldState);
                var index = m_Logic.ValueRO.Def.StateMapping[state].Index;
                ref var mapState = ref m_WorldStates.ElementAt(index);
                
                if (Def.TryGetAction(m_Logic.ValueRO.m_Action, out GoapAction action) &&
                    action.GetGoalTools().LeadsToGoal(state))
                {
#if LOGIC_DEBUG
                    UnityEngine.Debug.Log($"{Self}({SelfName}) {System.UpdateCount} [Logic] {Action} - done");
#endif
                    m_Logic.ValueRW.m_Work = false;
                }

                m_Logic.ValueRW.m_WaitChangeWorld = false;
            }

            public bool HasWorldState<T>(T worldState, bool value)
                where T : struct, IConvertible
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
                where T : IAction
            {
                SetEvent(LogicActionHandle.From<T>());
            }

            public void SetEvent(LogicActionHandle value)
            {
                if (Def.TryGetAction(value, out GoapAction action) && action.CanTransition(m_WorldStates, Def))
                {
                    m_Logic.ValueRW.m_Action = value;
                    m_Logic.ValueRW.m_Work = false;
#if LOGIC_DEBUG
                    UnityEngine.Debug.Log($"{Self}({SelfName}) {System.UpdateCount} [Logic] new event \"{Action}\"");
#endif
                }
            }

            public void ExecuteAction<TContext>(TContext context)
                where TContext: ILogicContext
            {
                Def.ActionExecute(context, m_Logic.ValueRO.m_Action);
            }
        }
    }
}