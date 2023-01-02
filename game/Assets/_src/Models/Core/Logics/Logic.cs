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
        private readonly Def<Config> m_Def;
        
        private int m_State;
        public int m_Result;

        public bool Work;
        public int StateID => m_State;

        public bool IsValid => m_Def.Value.IsValid;

        [CreateProperty]
        public Enum State => GetValue(m_State);
        [CreateProperty]
        public Enum Result => GetValue(m_Result);

        public Enum GetValue(int id)
        {
            return m_Def.Value.GetState(id);
        }

        public Logic(Def<Config> config)
        {
            m_Def = config;
            m_State = 0;
            m_Result = 0;
            Work = false;
        }

        public void SetStateID(int value)
        {
            m_State = value;
        }

        public void SetResult(Enum value)
        {
            m_Result = Config.GetID(value);
        }

        public int GetNextStateID(Enum result)
        {
            return m_Def.Value.GetNextStateID(ref this, result);
        }
    }
}