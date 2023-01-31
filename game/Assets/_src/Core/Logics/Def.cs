using System;
using Unity.Entities;
using Common.Defs;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;

namespace Game.Model.Logics
{
    public partial struct Logic : IComponentData, IDefineable
    {
        [Serializable]
        public partial class LogicDef : IDef<Logic>
        {
            private Dictionary<LogicHandle, GoapAction> m_Actions = new Dictionary<LogicHandle, GoapAction>();
            private Map<LogicHandle, LogicHandle> m_Effects = new Map<LogicHandle, LogicHandle>(true);

            private LogicHandle m_Goal;
            private bool m_GoalValue;

            public (LogicHandle goal, bool value) GetGoal()
            {
                return (m_Goal, m_GoalValue);
            }

            ~LogicDef()
            {
                m_Effects.Dispose();
            }

            public ConfigAction AddAction(Enum value)
            {
                var config = new ConfigAction(value, (ref GoapAction action) =>
                {
                    m_Actions.Add(action.Handle, action);
                }, m_Effects);
                return config;
            }

            public IEnumerable<LogicHandle> GetActionsFromGoal(LogicHandle goal)
            {
                m_Effects.TryGetValues(goal, out IEnumerable<LogicHandle> values);
                return values;
            }

            public void AddGoal(Enum goal, bool value)
            {
                m_Goal = LogicHandle.FromEnum(goal);
                m_GoalValue = value;
            }

            public NativeArray<GoapAction> GetActions(Allocator allocator)
            {
                return new NativeArray<GoapAction>(m_Actions.Values.ToArray(), allocator);
            }

            public class ConfigAction
            {
                public delegate void AddAction(ref GoapAction action);

                private GoapAction m_Action;
                private ref GoapAction Action => ref m_Action;
                private Map<LogicHandle, LogicHandle> m_Hash;

                public ConfigAction(Enum value, AddAction callback, Map<LogicHandle, LogicHandle> hash)
                {
                    m_Hash = hash;
                    m_Action = new GoapAction(LogicHandle.FromEnum(value));
                    callback(ref Action);
                }

                public ConfigAction AddPreconditions(Enum condition, bool value)
                {
                    m_Action.GetWriter().AddPreconditions(LogicHandle.FromEnum(condition), value);
                    return this;
                }

                public ConfigAction AddEffect(Enum effect, bool value)
                {
                    var handle = LogicHandle.FromEnum(effect);
                    m_Hash.Add(handle, Action.Handle);
                    m_Action.GetWriter().AddEffect(handle, value);
                    return this;
                }
            }
        }
    }
}