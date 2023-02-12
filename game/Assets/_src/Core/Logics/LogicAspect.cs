using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Properties;

using static Game.Model.Logics.Logic;

namespace Game.Model.Logics
{
    public readonly partial struct LogicAspect : IAspect
    {
        private readonly Entity m_Self;
        private readonly RefRW<Logic> m_Logic;
        private readonly DynamicBuffer<LogicHandle> m_Plan;
        private readonly DynamicBuffer<Logic.WorldState> m_WorldStates;
        private readonly RefRO<Root> m_Root;

        public Entity Self => m_Self;
        public Logic.LogicDef Def => m_Logic.ValueRO.Def;
        [CreateProperty]
        public string CurrentAction => m_Logic.ValueRO.CurrentAction.ToString();
        [CreateProperty]
        public bool IsWork => m_Logic.ValueRO.IsWork;
        public bool IsValid => m_Logic.ValueRO.Def.IsValid;
        public bool HasPlan => m_Plan.Length > 0;

        /*
        public bool HasState(LogicHandle worldState, bool value)
        {
            return GetState(worldState) == value;
        }
        */

        public bool IsAction()
        {
            return m_Logic.ValueRO.CurrentAction != LogicHandle.Null;
        }

        public bool IsCurrentAction(Enum action)
        {
            return m_Logic.ValueRO.CurrentAction == LogicHandle.FromEnumTest(action);
        }

        public bool IsCurrentAction(LogicHandle action)
        {
            return m_Logic.ValueRO.CurrentAction == action;
        }

        public void SetWorldState(Enum worldState, bool value)
        {
            var index = m_Logic.ValueRO.Def.StateMapping[LogicHandle.FromEnumTest(worldState)].Index;
            m_WorldStates.ElementAt(index).Value = value;
            m_Logic.ValueRW.SetDone();
            UnityEngine.Debug.Log($"{Self} [Logic] change world: {worldState} - {value}");
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

        public void SetPlan(NativeArray<LogicHandle> plan)
        {
            m_Plan.CopyFrom(plan);
        }

        public bool IsActionSuccess()
        {
            var action = Def.GetAction(m_Logic.ValueRO.CurrentAction);
            return action.IsSuccess(m_WorldStates, Def);
        }

        public void SetFailed()
        {
            m_Logic.ValueRW.SetAction(LogicHandle.Null);
            m_Logic.ValueRW.SetDone();
            //m_WorldStates.ElementAt(0).Value = m_WorldStates[0].Value;
        }

        public void SetAction(LogicHandle value)
        {
            var action = Def.GetAction(value);
            if (!action.CanTransition(m_WorldStates, Def))
                SetFailed();
            else
                m_Logic.ValueRW.SetAction(value);
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