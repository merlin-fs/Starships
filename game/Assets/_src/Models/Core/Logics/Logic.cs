using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Common.Defs;
using Unity.Mathematics;

namespace Game.Model
{
    using Result = ILogic.Result;

    public readonly partial struct LogicAspect : IAspect
    {
        public readonly Entity Self;

        private readonly RefRW<Logic> m_Logic;

        [CreateProperty]
        public string State => m_Logic.ValueRO.State != null 
            ? Enum.GetName(m_Logic.ValueRO.State.GetType(), m_Logic.ValueRO.State) 
            : "null";
        
        [CreateProperty]
        public Result Result => m_Logic.ValueRO.Result;

        public void SetResult(Result result)
        {
            m_Logic.ValueRW.Result = result;
        }
        public void SetStateID(int2 value)
        {
            m_Logic.ValueRW.SetStateID(value);
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

    public partial struct Logic : IComponentData, IDefineable
    {
        [DontSerialize]
        private readonly Def<Config> m_Config;
        private int2 m_State;
        public Result Result;
        public Enum State => m_Config.Value.GetState(m_State);

        [CreateProperty]
        public string StateName => State != null
            ? Enum.GetName(State.GetType(), State)
            : "null";

        public int2 StateID => m_State;

        public Logic(Def<Config> config)
        {
            m_Config = config;
            m_State = 0;
            Result = Result.Done;
        }

        public void SetStateID(int2 value)
        {
            m_State = value;
        }

        public int2 GetNextStateID(Result result)
        {
            return m_Config.Value.GetNextStateID(ref this, result);
        }
    }
}