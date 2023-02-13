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
        public LogicDef Def => m_Def.Value;

        public LogicHandle Action;
        public bool Work;
        public bool WaitNewGoal;
        public bool WaitChangeWorld;
        public Logic(Def<LogicDef> def)
        {
            m_Def = def;
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