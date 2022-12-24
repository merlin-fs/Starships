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
        public Enum State => m_Logic.ValueRO.State;
        
        [CreateProperty]
        public Result Result => m_Logic.ValueRO.Result;

        [CreateProperty]
        public int JobIndex => m_Logic.ValueRO.JobIndex;


        public void SetResult(Result result, int jobIndex)
        {
            m_Logic.ValueRW.Result = result;
            m_Logic.ValueRW.JobIndex = jobIndex;
        }
        public void SetStateID(int2 value)
        {
            m_Logic.ValueRW.SetStateID(value);
        }

        public int2 GetNextStateID(out int transitionIndex)
        {
            return m_Logic.ValueRO.GetNextStateID(Result, out transitionIndex);
        }
    }

    public partial struct Logic : IComponentData, IDefineable
    {
        [DontSerialize]
        private readonly Def<Config> m_Config;

        private int2 m_State;
        public Result Result;
        public int JobIndex;

        [CreateProperty]
        public Enum State => m_Config.Value.GetState(m_State);

        public Logic(Def<Config> config)
        {
            m_Config = config;
            m_State = 0;
            Result = Result.Done;
            JobIndex = -1;
        }

        public void SetStateID(int2 value)
        {
            m_State = value;
        }

        public int2 GetNextStateID(Result result, out int transitionIndex)
        {
            return m_Config.Value.GetNextStateID(ref this, result, out transitionIndex);
        }
    }
}