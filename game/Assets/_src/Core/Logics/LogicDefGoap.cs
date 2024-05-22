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
            private Dictionary<LogicActionHandle, ConfigTransition> m_Transitions = new ();
            private Map<GoalHandle, LogicActionHandle> m_Effects = new (10, Allocator.Persistent, true);
            private Dictionary<EnumHandle, WorldActionData> m_StateMapping = new (10);
            private List<Goal> m_Goal = new ();
            private Dictionary<LogicActionHandle, IAction> m_Actions = new();
           
            ~LogicDef()
            {
                m_Effects.Dispose();
            }

            public Dictionary<EnumHandle, WorldActionData> StateMapping => m_StateMapping;
            public IEnumerable<Goal> Goals => m_Goal.Reverse<Goal>();

            public ConfigTransition AddTransition<S, T>()
                where S: IStateData
                where T: IAction
            {
                var config = new ConfigTransition(
                    LogicHandle.From<S>(), typeof(T), 
                    m_Effects, 
                    (handle, type) => m_Actions.Add(handle, (IAction)Activator.CreateInstance(type)));
                m_Transitions.Add(config.Action.Handle, config);
                
                return config;
            }

            public IEnumerable<LogicActionHandle> GetActionsFromGoal(GoalHandle goal)
            {
                m_Effects.TryGetValues(goal, out IEnumerable<LogicActionHandle> values);
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

            public bool TryGetAction(LogicActionHandle handle, out GoapAction action)
            {
                if (m_Transitions.TryGetValue(handle, out ConfigTransition config))
                {
                    action = config.Action;
                    return true;
                }
                action = default;
                return false;
            }

            public void SetWorldState<T>(T state, bool value)
                where T: struct, IConvertible
            {
                var handle = EnumHandle.FromEnum(state);
                var data = m_StateMapping[handle];
                data.Initialize = value;
                m_StateMapping[handle] = data;
            }

            public NativeArray<GoapAction> GetActions(Allocator allocator)
            {
                return new NativeArray<GoapAction>(m_Transitions.Values.Select(c => c.Action).ToArray(), allocator);
            }

            public struct WorldActionData
            {
                public bool Initialize;
                public int Index;
            }

            public class ConfigTransition
            {
                private GoapAction m_Action;
                public ref GoapAction Action => ref m_Action;
                private Map<GoalHandle, LogicActionHandle> m_Hash;
                private Action<LogicActionHandle, Type> m_AddAction;

                public ConfigTransition(LogicHandle logicHandle, Type value, 
                    Map<GoalHandle, LogicActionHandle> hash, Action<LogicActionHandle, Type> addAction)
                {
                    m_Hash = hash;
                    var handle = LogicActionHandle.FromType(value);
                    m_Action = new GoapAction(logicHandle, handle);
                    m_AddAction = addAction;
                    m_AddAction.Invoke(handle, value);
                }

                //TODO: сделать чтобы можно было добавлять только "enum State"
                public ConfigTransition AddPreconditions<T>(T condition, bool value)
                    where T: struct, IConvertible
                {
                    m_Action.GetWriter().AddPreconditions(EnumHandle.FromEnum(condition), value);
                    return this;
                }

                public ConfigTransition AddAction<S, T>()
                    where S: IStateData
                    where T: IAction
                {
                    var handle = LogicActionHandle.From<T>();
                    m_Action.GetWriter().AddAction(LogicHandle.From<S>(), handle);
                    m_AddAction.Invoke(handle, typeof(T));
                    return this;
                }

                //TODO: сделать чтобы можно было добавлять только "enum State"
                public ConfigTransition AddEffect<T>(T effect, bool value)
                    where T: struct, IConvertible
                {
                    var handle = EnumHandle.FromEnum(effect);
                    m_Hash.Add(GoalHandle.FromHandle(handle, value), Action.Handle);
                    m_Action.GetWriter().AddEffect(handle, value);
                    return this;
                }

                public ConfigTransition Cost(float value)
                {
                    m_Action.GetWriter().SetCost(value);
                    return this;
                }
            }
        }
    }
}