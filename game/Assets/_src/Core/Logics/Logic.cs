using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Common.Defs;

using Game.Core.Saves;

namespace Game.Model.Logics
{
    public partial struct Logic : IComponentData, IDefinable, IDefineableCallback
    {
        private readonly RefLink<LogicDef> m_RefLink;
        public LogicDef Def => m_RefLink.Value;

        public LogicHandle Action;
        public bool Active;
        public bool Work;
        public bool WaitNewGoal;
        public bool WaitChangeWorld;
        
        public Logic(RefLink<LogicDef> refLink)
        {
            m_RefLink = refLink;
            Active = true;
            Work = false;
            WaitNewGoal = false;
            WaitChangeWorld = false;
            Action = LogicHandle.Null;
        }
        #region IDefineableCallback
        void IDefineableCallback.AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddBuffer<LogicHandle>(entity);
            var goals = context.AddBuffer<Goal>(entity);
            foreach (var iter in Def.Goals)
                goals.Add(iter);

            var buff = context.AddBuffer<WorldState>(entity);
            buff.ResizeUninitialized(m_RefLink.Value.StateMapping.Count);
            foreach (var iter in m_RefLink.Value.StateMapping)
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