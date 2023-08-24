using System;
using Common.Defs;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;
using Game.Core;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        [Serializable]
        public partial class LogicDef : IDef<Logic>
        {
            private Dictionary<EnumHandle, ConfigAction> m_Actions = new Dictionary<EnumHandle, ConfigAction>();
            private Map<GoalHandle, EnumHandle> m_Effects = new Map<GoalHandle, EnumHandle>(10, Allocator.Persistent, true);
            private Dictionary<EnumHandle, WorldActionData> m_StateMapping = new Dictionary<EnumHandle, WorldActionData>(10);
            private List<Goal> m_Goal = new List<Goal>();

            ~LogicDef()
            {
                m_Effects.Dispose();
            }

            public Dictionary<EnumHandle, WorldActionData> StateMapping => m_StateMapping;
            public IEnumerable<Goal> Goals => m_Goal.Reverse<Goal>();
            public ConfigAction AddAction<T>(T value)
                where T: struct, IConvertible
            {
                var config = new ConfigAction(EnumHandle.FromEnum(value), m_Effects);
                m_Actions.Add(config.Action.Handle, config);
                return config;
            }

            public IEnumerable<EnumHandle> GetActionsFromGoal(GoalHandle goal)
            {
                m_Effects.TryGetValues(goal, out IEnumerable<EnumHandle> values);
                return values;
            } 

            public void EnqueueGoal<T>(T goal, bool value)
                where T: struct, IConvertible
            {
                m_Goal.Add(new Goal 
                { 
                    State = EnumHandle.FromEnum(goal), 
                    Value = value, 
                    Repeat = false, 
                });
            }

            public void EnqueueGoalRepeat<T>(T goal, bool value)
                where T: struct, IConvertible
            {
                m_Goal.Add(new Goal
                {
                    State = EnumHandle.FromEnum(goal),
                    Value = value,
                    Repeat = true,
                });
            }

            public bool TryGetAction(EnumHandle handle, out GoapAction action)
            {
                if (m_Actions.TryGetValue(handle, out ConfigAction config))
                {
                    action = config.Action;
                    return true;
                }
                action = default;
                return false;
            }

            public void SetInitializeState<T>(T state, bool value)
                where T: struct, IConvertible
            {
                var handle = EnumHandle.FromEnum(state);
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
                private Map<GoalHandle, EnumHandle> m_Hash;

                public ConfigAction(EnumHandle value, Map<GoalHandle, EnumHandle> hash)
                {
                    m_Hash = hash;
                    m_Action = new GoapAction(value);
                }

                public ConfigAction AddPreconditions<T>(T condition, bool value)
                    where T: struct, IConvertible
                {
                    m_Action.GetWriter().AddPreconditions(EnumHandle.FromEnum(condition), value);
                    return this;
                }

                public ConfigAction AddEffect<T>(T effect, bool value)
                    where T: struct, IConvertible
                {
                    var handle = EnumHandle.FromEnum(effect);
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