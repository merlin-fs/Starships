using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Mathematics;

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
        public bool IsWork => m_Logic.ValueRO.Work;
        [CreateProperty]
        public bool IsValid => m_Logic.ValueRO.IsValid;

        public bool IsSupports(int logicID)
        {
            return m_Logic.ValueRO.LogicID == logicID;
        }

        public void SetResult(Enum result)
        {
            m_Logic.ValueRW.SetResult(result);
            m_Logic.ValueRW.Work = false;
        }

        public void SetStateID(int value)
        {
            m_Logic.ValueRW.SetStateID(value);
            m_Logic.ValueRW.Work = true;
        }

        public int GetNextStateID()
        {
            return m_Logic.ValueRO.GetNextStateID(Result);
        }

        public bool Equals(Enum @enum)
        {
            return m_Logic.ValueRO.StateID == Logic.Config.GetID(@enum);
        }
    }
}