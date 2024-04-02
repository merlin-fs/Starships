using System;
using Unity.Entities;
using Unity.Properties;
using Common.Defs;
using Game.Core;

namespace Game.Model.Logics
{
    public partial struct Logic : IComponentData, IDefinable, IDefinableCallback
    {
        [CreateProperty] private string Action => m_Action.ToString();
        [CreateProperty] private bool Work => m_Work;
        [CreateProperty] private bool WaitNewGoal => m_WaitNewGoal;

        private readonly RefLink<LogicDef> m_RefLink;
        private LogicDef Def => m_RefLink.Value;
        private EnumHandle m_Action;
        private bool m_Active;
        private bool m_Work;
        private bool m_WaitNewGoal;
        private bool m_WaitChangeWorld;
        private bool m_Event;
        
        public Logic(RefLink<LogicDef> refLink)
        {
            m_RefLink = refLink;
            m_Active = false;
            m_Event = false;
            m_Work = true;
            m_WaitNewGoal = false;
            m_WaitChangeWorld = false;
            m_Action = refLink.Value.InitializeAction;
        }

        private readonly bool IsCurrentAction(EnumHandle action)
        {
            return m_Active && EnumHandle.Equals(m_Action, action);
        }
        #region IDefineableCallback
        void IDefinableCallback.AddComponentData(Entity entity, IDefinableContext context)
        {
            context.AddComponentData(entity, new InitTag());
            context.AddBuffer<Plan>(entity);
            context.AddBuffer<WorldChanged>(entity);
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

        void IDefinableCallback.RemoveComponentData(Entity entity, IDefinableContext context)
        {

        }
        #endregion
    }
}