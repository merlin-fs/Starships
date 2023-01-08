using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Common.Defs;
using Unity.Mathematics;
using static UnityEngine.Rendering.DebugUI;

namespace Game.Model.Logics
{
    [Serializable]
    public partial struct Logic : IComponentData, IDefineable
    {
        [DontSerialize]
        private readonly Def<LogicDef> m_Def;
        
        private int m_State;
        public int m_Result;
        private int m_LogicID;
        private bool m_Work;

        public int StateID => m_State;
        public bool IsValid => m_Def.Value.IsValid;
        public int LogicID => m_LogicID;
        public bool IsWork => m_Work;

        [CreateProperty]
        public Enum State => GetValue(m_State);
        [CreateProperty]
        public Enum Result => GetValue(m_Result);

        public Enum GetValue(int id)
        {
            return m_Def.Value.GetState(id);
        }

        public Logic(Def<LogicDef> def)
        {
            m_Def = def;
            m_Work = false;
            def.Value.TryGetID(null, out m_State);
            def.Value.TryGetID(null, out m_Result);
            m_LogicID = def.Value.LogicID;
        }

        public void SetState(int value)
        {
            m_State = value;
            m_Work = true;
        }

        public int GetNextState()
        {
            return m_Def.Value.GetNextState(ref this, m_Result);
        }

        public int GetNextState(int value)
        {
            return m_Def.Value.GetNextState(ref this, value);
        }

        public bool HasState(Enum value)
        {
            return m_Def.Value.TryGetID(value, out int _);
        }

        public void TrySetState(Enum value)
        {
            if (m_Def.Value.TryGetID(value, out int id))
                SetState(id);
        }

        public void TrySetResult(Enum value)
        {
            if (m_Def.Value.TryGetID(value, out int id))
            {
                m_Result = id;
                m_Work = false;
            }
            else
                throw new ArgumentException($"{value} is not result this machine");
        }
    }
}