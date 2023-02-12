using System;
using Common.Defs;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        [Serializable]
        public partial class LogicDef : IDef<Logic>
        {
            private Dictionary<LogicHandle, ConfigAction> m_Actions = new Dictionary<LogicHandle, ConfigAction>();
            private Map<GoalHandle, LogicHandle> m_Effects = new Map<GoalHandle, LogicHandle>(10, Allocator.Persistent, true);
            private Dictionary<LogicHandle, WorldActionData> m_StateMapping = new Dictionary<LogicHandle, WorldActionData>(10);
            private States m_Goal = new States(Allocator.Persistent);

            ~LogicDef()
            {
                m_Effects.Dispose();
                m_Goal.Dispose();
            }

            public Dictionary<LogicHandle, WorldActionData> StateMapping => m_StateMapping;

            public States GetGoal()
            {
                return m_Goal;
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
                m_Goal.SetState(goal, value);
            }

            public GoapAction GetAction(LogicHandle handle)
            {
                return m_Actions[handle].Action;
            }

            public void SetInitializeState(Enum state, bool value)
            {
                var handle = LogicHandle.FromEnum(state);
                var data = m_StateMapping[handle];
                data.Initialize = value;
                m_StateMapping[handle] = data;
            }

            public NativeArray<GoapAction> GetActions(Allocator allocator)
            {
                return new NativeArray<GoapAction>(m_Actions.Values.Select(c => c.Action).ToArray(), allocator);
            }

            public struct WorldActionData
            {
                public bool Initialize;
                public int Index;
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