using System;
using Common.Defs;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        [Serializable]
        public class Config : IDef<Logic>
        {
            private Dictionary<int2, StateInfo> m_States = new Dictionary<int2, StateInfo>();
            public Configuration Configure() => new Configuration(this);

            public int2 GetNextStateID(ref Logic logic, Result result)
            {
                var info = GetInfo(logic.CurrentState);
                var list = info.GetTransitions(result);
                var next = list.RandomElement();
                return next == null 
                    ? new int2() 
                    : next.ID;
            }

            public Enum GetState(int2 value)
            {
                return math.all(value == int2.zero) 
                    ? null 
                    : m_States[value].Value;
            }

            public static int2 GetID(Enum value)
            {
                return value == null 
                    ? new int2() 
                    : new int2(value.GetType().GetHashCode(), value.GetHashCode());
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

            private void AddTransition(Result result, Enum _from, Enum _to)
            {
                StateInfo from = GetInfo(_from, true);
                StateInfo to = GetInfo(_to, true);
                from.AddTransition(to, result);
            }

            public class Configuration
            {
                private Config m_Owner;

                public Configuration(Config owner)
                {
                    m_Owner = owner;
                }

                public Configuration Transition(Result result, Enum from, Enum to)
                {
                    m_Owner.AddTransition(result, from, to);
                    return this;
                }
            }

            [Serializable]
            private class StateInfo
            {
                public int2 ID { get; }
                public Enum Value { get; }

                private readonly List<StateInfo> m_Done = new List<StateInfo>();
                private readonly List<StateInfo> m_Error = new List<StateInfo>();

                public StateInfo(Enum value)
                {
                    Value = value;
                    ID = GetID(value);
                }

                public void AddTransition(StateInfo info, Result result)
                {
                    switch (result)
                    {
                        case Result.Done: m_Done.Add(info); break;
                        case Result.Error: m_Error.Add(info); break;
                    }
                }

                public IEnumerable<StateInfo> GetTransitions(Result result)
                {
                    return result switch
                    {
                        Result.Done => m_Done,
                        Result.Error => m_Error,
                        _ => throw new NotImplementedException(),
                    };
                }
            }
        }
    }
}