using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Serialization;
using Common.Defs;

using Game.Core;
using Game.Core.Saves;

namespace Game.Model.Logics
{
    public partial struct Logic : IComponentData, IDefinable, IDefineableCallback
    {
        [CreateProperty] private string Action => m_Action.ToString();
        [CreateProperty] private bool Work => m_Work;
        [CreateProperty] private bool WaitNewGoal => m_WaitNewGoal;
        [CreateProperty] private bool WaitChangeWorld => m_WaitChangeWorld;

        private readonly RefLink<LogicDef> m_RefLink;
        private LogicDef Def => m_RefLink.Value;
        private EnumHandle m_Action;
        private bool m_Active;
        private bool m_Work;
        private bool m_WaitNewGoal;
        private bool m_WaitChangeWorld;
        
        public Logic(RefLink<LogicDef> refLink)
        {
            m_RefLink = refLink;
            m_Active = true;
            m_Work = false;
            m_WaitNewGoal = false;
            m_WaitChangeWorld = false;
            m_Action = EnumHandle.Null;
        }

        private readonly bool IsCurrentAction(EnumHandle action)
        {
            return m_Active && EnumHandle.Equals(m_Action, action);
        }
        #region IDefineableCallback
        void IDefineableCallback.AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddBuffer<EnumHandle>(entity);
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