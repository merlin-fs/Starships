using System;
using Common.Defs;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Unity.Mathematics;
using NMemory.DataStructures;
using Unity.Entities;
using UnityEngine;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public interface IConfigurator
        {
            void Init(Config config);
        }

        [Serializable]
        public class Config : IDef<Logic>
        {
            [SerializeField, SelectType(typeof(IConfigurator))]
            private string m_Configurator;
            private static ConcurrentDictionary<Enum, int> m_AllStates = new ConcurrentDictionary<Enum, int>();

            Dictionary<int, StateInfo> m_States = new Dictionary<int, StateInfo>();
            public Configuration Configure() => new Configuration(this);

            public void Init()
            {
                var type = Type.GetType(m_Configurator);
                LogicConcreteSystem.AddInit(this, type);
            }

            public int GetNextStateID(ref Logic logic, Enum result)
            {
                var info = GetInfo(logic.State);
                var list = info.GetTransitions(result);
                var next = list.RandomElement();
                return next == null 
                    ? 0
                    : next.ID;
            }

            public Enum GetState(int value)
            {
                return (value == 0) 
                    ? null 
                    : m_States[value].Value;
            }

            public static int GetID(Enum value)
            {
                if (value == null)
                    return 0;

                if (!m_AllStates.TryGetValue(value, out var id))
                {
                    id = value != null
                        ? new int2(value.GetType().GetHashCode(), value.GetHashCode()).GetHashCode()
                        : 0;
                    m_AllStates.TryAdd(value, id);
                }
                return id;
            }

            private StateInfo GetInfo(Enum state, bool need = false)
            {
                var id = GetID(state);
                if (!m_States.TryGetValue(id, out StateInfo info) && need)
                {
                    info = new StateInfo(state);
                    m_States.Add(id, info);
                }
                return info;
            }

            private void AddTransition(Enum _from, Enum _result, Enum _to)
            {
                StateInfo from = GetInfo(_from, true);
                StateInfo to = GetInfo(_to, true);
                StateInfo result = GetInfo(_result, true);

                from.AddTransition(to, result.ID);
            }

            public class Configuration
            {
                private Config m_Owner;

                public Configuration(Config owner)
                {
                    m_Owner = owner;
                }

                public Configuration Transition(Enum from, Enum result, Enum to)
                {
                    m_Owner.AddTransition(from, result, to);
                    return this;
                }
            }

            [Serializable]
            private class StateInfo
            {
                public int ID { get; }
                public Enum Value { get; }

                private readonly RedBlackTree<int, StateInfo> m_Transition = new RedBlackTree<int, StateInfo>();

                public StateInfo(Enum value)
                {
                    Value = value;
                    ID = GetID(value);
                }

                public void AddTransition(StateInfo info, int resultID)
                {
                    m_Transition.Insert(resultID, info);
                }

                public IEnumerable<StateInfo> GetTransitions(Enum result)
                {
                    var select = m_Transition.Select(GetID(result));
                    return select;
                }
            }
        }
    }
}