using System;
using Unity.Entities;
using Unity.Properties;
using Common.Defs;
using Game.Core;

using UnityEngine;

namespace Game.Model.Logics
{
    public partial struct Logic : IComponentData, IEnableableComponent, 
        IDefinable, IDefinableCallback, Logic.IStateData
    {
        [CreateProperty] private string Action => m_Action.ToString();
        [CreateProperty] private bool Work => m_Work;
        [CreateProperty] private bool WaitNewGoal => m_WaitNewGoal;

        private readonly RefLink<LogicDef> m_RefLink;
        private LogicDef Def => m_RefLink.Value;
        private LogicActionHandle m_Action;
        private bool m_Work;
        private bool m_WaitNewGoal;
        private bool m_WaitChangeWorld;
        
        public Logic(RefLink<LogicDef> refLink)
        {
            m_RefLink = refLink;
            m_Work = false;
            m_WaitNewGoal = false;
            m_WaitChangeWorld = false;
            m_Action = default;
        }
        
        #region IDefineableCallback
        void IDefinableCallback.AddComponentData(Entity entity, IDefinableContext context)
        {
            context.AddComponentData(entity, new ChangeTag());
            context.AddComponentData(entity, new Logic());
            context.SetComponentEnabled<Logic>(entity, false);
            
            context.AddBuffer<Plan>(entity);
            context.AddBuffer<WorldChange>(entity);
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