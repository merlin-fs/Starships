using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Common.Defs;

namespace Game.Model.Logics
{
    [Serializable]
    public partial struct Logic : IComponentData, IDefineable
    {
        [DontSerialize]
        private readonly Def<LogicDef> m_Def;
        
        private LogicHandle m_Action;
        private bool m_Work;

        [CreateProperty]
        public LogicHandle CurrentAction => m_Action;
        [CreateProperty]
        public bool IsWork => m_Work;

        public Logic(Def<LogicDef> def)
        {
            m_Def = def;
            m_Work = false;
            m_Action = LogicHandle.Null;
        }

        public void SetState(LogicHandle value)
        {
            m_Action = value;
            m_Work = true;
        }

        public void SetDone()
        {
            m_Work = false;
        }

        public LogicHandle GetNextState()
        {
            return m_Def.Value.GetNextAction(ref this);
        }
    }
}