using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Common.Defs;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    [Serializable]
    public partial struct Logic : IComponentData, IDefineable
    {
        [DontSerialize]
        private readonly Def<Config> m_Config;
        
        private int2 m_State;
        public Result CurrentResult;
        public bool Work;

        public Enum CurrentState => m_Config.Value.GetState(m_State);

        [CreateProperty]
        public string StateName => CurrentState != null
            ? Enum.GetName(CurrentState.GetType(), CurrentState)
            : "null";

        public int2 StateID => m_State;

        public Logic(Def<Config> config)
        {
            m_Config = config;
            m_State = 0;
            CurrentResult = Result.Done;
            Work = false;
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