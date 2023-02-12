using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Common.Defs;

namespace Game.Model.Logics
{
    [Serializable]
    [WriteGroup(typeof(WorldState))] 
    public partial struct Logic : IComponentData, IDefineable, IDefineableCallback
    {
        [DontSerialize]
        private readonly Def<LogicDef> m_Def;
        private LogicHandle m_Action;
        private bool m_Work;
        public LogicDef Def => m_Def.Value;
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

        public void SetAction(LogicHandle value)
        {
            m_Action = value;
            m_Work = true;
        }

        public void SetDone()
        {
            m_Work = false;
        }
        #region IDefineableCallback
        void IDefineableCallback.AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddBuffer<LogicHandle>(entity);
            context.AddBuffer<WaitState>(entity);

            var buff = context.AddBuffer<WorldState>(entity);
            buff.ResizeUninitialized(m_Def.Value.StateMapping.Count);
            foreach (var iter in m_Def.Value.StateMapping)
            {
                buff[iter.Value.Index] = new WorldState { Value = iter.Value.Initialize, };
            }
        }

        void IDefineableCallback.RemoveComponentData(Entity entity, IDefineableContext context)
        {

        }
        #endregion
    }
}