using System;
using Unity.Entities;
using Unity.Properties;

namespace Game.Model.Logics
{
    public readonly partial struct LogicAspect : IAspect
    {
        private readonly Entity m_Self;
        public Entity Self => m_Self;
        private readonly RefRW<Logic> m_Logic;

        #region DesignTime

#if UNITY_EDITOR
        [CreateProperty]
        public string StateName => m_Logic.ValueRO.State?.ToString();
        [CreateProperty]
        public string ResultName => m_Logic.ValueRO.Result?.ToString();

#endif
        #endregion
        public Enum State => m_Logic.ValueRO.State;
        public Enum Result => m_Logic.ValueRO.Result;
       
        [CreateProperty]
        public bool IsWork => m_Logic.ValueRO.IsWork;
        [CreateProperty]
        public bool IsValid => m_Logic.ValueRO.IsValid;

        public bool IsSupports(int logicID)
        {
            return m_Logic.ValueRO.LogicID == logicID;
        }

        public bool HasState(Enum value)
        {
            return m_Logic.ValueRO.HasState(value);
        }

        public void TrySetResult(Enum result)
        {
            m_Logic.ValueRW.TrySetResult(result);
        }

        public void TrySetState(Enum state)
        {
            m_Logic.ValueRW.TrySetState(state);
        }

        public void SetState(int value)
        {
            m_Logic.ValueRW.SetState(value);
        }

        public int GetNextState()
        {
            return m_Logic.ValueRO.GetNextState();
        }
    }
}