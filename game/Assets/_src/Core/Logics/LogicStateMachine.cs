using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Logics
{

    public partial struct Logic
    {
        public partial class LogicDef
        {
            private readonly HashSet<Type> m_SupportSystems = new HashSet<Type>();
            public bool IsValid => m_Actions.Count > 0;

            public bool IsSupportSystem(IJobEntity job) 
            {
                var type = job.GetType();
                if (type.IsNested)
                    type = type.ReflectedType;
                return m_SupportSystems.Contains(type);
            }

            public void AddSupportSystem([NotNull]Type typeSystem)
            {
                Assert.IsNotNull(typeSystem);
                m_SupportSystems.Add(typeSystem);
            }

            public void Init()
            {
                m_StateMapping.Clear();
                m_Actions.Clear();
                m_Goal.Clear();
                m_Effects.Dispose();
                m_Effects = new Map<GoalHandle, LogicHandle>(10, Allocator.Persistent, true);
                
                var types = typeof(IStateData).GetDerivedTypes(true)
                    .SelectMany(t => t.GetNestedTypes())
                    .Where(t => t.IsEnum && t.Name == "State");

                foreach (var iter in types)
                {
                    foreach (var e in iter.GetEnumValues())
                    {
                        var value = LogicHandle.FromEnum((Enum)e);
                        m_StateMapping.Add(value, new WorldActionData { Index = m_StateMapping.Count });
                    }
                }
            }
        }

        /*
        public interface IConfigurator
        {
            void Init(LogicDef def);
        }
        */
        /*
        [Serializable]
        public class LogicDef : IDef<Logic>
        {
            private enum InternalType
            {
                Null,
            }

            [SerializeField, SelectType(typeof(IConfigurator))]
            private string m_Configurator;

            Dictionary<int, StateInfo> m_States = new Dictionary<int, StateInfo>();
            Dictionary<Enum, StateInfo> m_IDs = new Dictionary<Enum, StateInfo>();
            public Configuration Configure() => new Configuration(this);

            private Type m_TypeSystem;
            public int LogicID => m_TypeSystem?.GetHashCode() ?? 0;

            public void Init()
            {
                m_TypeSystem = Type.GetType(m_Configurator);
                if (m_TypeSystem != null)
                    LogicConcreteSystem.AddInit(this, m_TypeSystem);
            }

            public bool IsValid => m_States.Count > 0;

            public int GetNextState(ref Logic logic, int resultId)
            {
                var info = GetInfo(logic.StateID);
                var list = info.GetTransitions(resultId);
                var next = list.RandomElement();
                return next == null 
                    ? m_IDs[null].ID
                    : next.ID;
            }

            public Enum GetState(int value)
            {
                return m_States[value].Value;
            }

            public bool TryGetID(Enum value, out int id)
            {
                value ??= null;
                id = -1;
                if (m_IDs.TryGetValue(value, out StateInfo info))
                {
                    id = info.ID; 
                    return true;
                }
                return false;
            }

            private StateInfo NeedInfo(Enum state)
            {
                state ??= null;
                StateInfo info = !m_IDs.ContainsKey(state) 
                    ? new StateInfo(this, state) 
                    : m_IDs[state];
                return info;
            }

            private StateInfo GetInfo(int id)
            {
                return m_States[id];
            }

            private void AddTransition(Enum _from, Enum _result, Enum _to)
            {
                StateInfo from = NeedInfo(_from);
                StateInfo to = NeedInfo(_to);
                StateInfo result = NeedInfo(_result);

                from.AddTransition(to, result.ID);
            }

            private void AddState(Enum _from)
            {
                StateInfo from = NeedInfo(_from);
            }

            public class Configuration
            {
                private LogicDef m_Owner;

                public Configuration(LogicDef owner)
                {
                    m_Owner = owner;
                }

                public Configuration Transition(Enum from, Enum result, Enum to)
                {
                    m_Owner.AddTransition(from, result, to);
                    return this;
                }

                public Configuration State(Enum from)
                {
                    m_Owner.AddState(from);
                    return this;
                }
            }

            [Serializable]
            private class StateInfo
            {
                public int ID { get; }
                public Enum Value { get; }

                private readonly RedBlackTree<int, StateInfo> m_Transition = new RedBlackTree<int, StateInfo>();

                public StateInfo(LogicDef owner, Enum value)
                {
                    Value = value;
                    ID = new int2(value.GetType().GetHashCode(), value.GetHashCode()).GetHashCode();
                    owner.m_IDs.Add(value, this);
                    owner.m_States.Add(ID, this);
                }

                public void AddTransition(StateInfo info, int resultID)
                {
                    m_Transition.Insert(resultID, info);
                }

                public IEnumerable<StateInfo> GetTransitions(int resultID)
                {
                    var select = m_Transition.Select(resultID);
                    return select;
                }
            }
        }
        */
    }
}