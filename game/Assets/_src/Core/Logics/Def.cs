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
            private Dictionary<LogicHandle, ConfigAction> m_Actions = new Dictionary<LogicHandle, ConfigAction>();
            private Map<GoalHandle, LogicHandle> m_Effects = new Map<GoalHandle, LogicHandle>(true);

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
                var config = new ConfigAction(value, m_Effects);
                m_Actions.Add(config.Action.Handle, config);
                return config;
            }

            public IEnumerable<LogicHandle> GetActionsFromGoal(GoalHandle goal)
            {
                m_Effects.TryGetValues(goal, out IEnumerable<LogicHandle> values);
                return values;
            } 

            public void AddGoal(Enum goal, bool value)
            {
                m_Goal = LogicHandle.FromEnum(goal);
                m_GoalValue = value;
            }

            public GoapAction GetAction(LogicHandle handle)
            {
                return m_Actions[handle].Action;
            }

            public NativeArray<GoapAction> GetActions(Allocator allocator)
            {
                return new NativeArray<GoapAction>(m_Actions.Values.Select(c => c.Action).ToArray(), allocator);
            }

            public class ConfigAction
            {
                public delegate void AddAction(ref GoapAction action);

                private GoapAction m_Action;
                public ref GoapAction Action => ref m_Action;
                private Map<GoalHandle, LogicHandle> m_Hash;

                public ConfigAction(Enum value, Map<GoalHandle, LogicHandle> hash)
                {
                    m_Hash = hash;
                    m_Action = new GoapAction(LogicHandle.FromEnum(value));
                }

                public ConfigAction AddPreconditions(Enum condition, bool value)
                {
                    m_Action.GetWriter().AddPreconditions(LogicHandle.FromEnum(condition), value);
                    return this;
                }

                public ConfigAction AddEffect(Enum effect, bool value)
                {
                    var handle = LogicHandle.FromEnum(effect);
                    m_Hash.Add(GoalHandle.FromHandle(handle, value), Action.Handle);
                    m_Action.GetWriter().AddEffect(handle, value);
                    return this;
                }

                public ConfigAction Cost(float value)
                {
                    m_Action.GetWriter().SetCost(value);
                    return this;
                }
            }
        }
    }
}