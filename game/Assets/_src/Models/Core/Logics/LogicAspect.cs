using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    using Result = Logic.Result;

    public readonly partial struct LogicAspect : IAspect
    {
        public readonly Entity Self;

        private readonly RefRW<Logic> m_Logic;

        [CreateProperty]
        public string State => m_Logic.ValueRO.CurrentState != null 
            ? Enum.GetName(m_Logic.ValueRO.CurrentState.GetType(), m_Logic.ValueRO.CurrentState) 
            : "null";
        
        [CreateProperty]
        public Result Result => m_Logic.ValueRO.CurrentResult;
        [CreateProperty]
        public bool IsWork => m_Logic.ValueRO.Work;

        public void SetResult(Result result)
        {
            m_Logic.ValueRW.CurrentResult = result;
            m_Logic.ValueRW.Work = false;
        }
        public void SetStateID(int2 value)
        {
            m_Logic.ValueRW.SetStateID(value);
            m_Logic.ValueRW.Work = true;
        }

        public int2 GetNextStateID()
        {
            return m_Logic.ValueRO.GetNextStateID(Result);
        }

        public bool Equals(Enum @enum)
        {
            return math.all(m_Logic.ValueRO.StateID == Logic.Config.GetID(@enum));
        }
    }
}