using System;
using Unity.Entities;
using Common.Defs;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model
{
    using Result = ILogic.Result;

    public partial struct Logic : IComponentData, IDefineable
    {
        [Serializable]
        public class Config : IDef<Logic>
        {
            private Dictionary<int2, StateInfo> m_States = new Dictionary<int2, StateInfo>();
            public Configuration Configure() => new Configuration(this);

            public int2 GetNextStateID(ref Logic logic, Result result, out int transitionIndex)
            {
                var info = GetInfo(logic.State);
                var list = info.GetTransitions(result);
                var next = list.RandomElement();
                transitionIndex = next.ID;
                return next.Info == null 
                    ? new int2() 
                    : next.Info.ID;
            }

            public Enum GetState(int2 value)
            {
                return m_States[value].Value;
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

            private void AddTransition<Transition>(Enum _from, Enum _to)
                where Transition : unmanaged, ILogicTransition
            {
                StateInfo from = GetInfo(_from, true);
                StateInfo to = GetInfo(_to, true);
                from.AddTransition<Transition>(to, Result.Done);
            }

            private void AddTransition(Enum _from, Enum _to)
            {
                StateInfo from = GetInfo(_from, true);
                StateInfo to = GetInfo(_to, true);
                from.AddTransition(to, Result.Done);
            }

            public class Configuration
            {
                private Config m_Owner;

                public Configuration(Config owner)
                {
                    m_Owner = owner;
                }

                public Configuration Transition<Transition>(Enum from, Enum to)
                    where Transition : unmanaged, ILogicTransition
                {
                    m_Owner.AddTransition<Transition>(from, to);
                    return this;
                }

                public Configuration Transition(Enum from, Enum to)
                {
                    m_Owner.AddTransition(from, to);
                    return this;
                }
            }

            private class StateInfo
            {
                public int2 ID { get; }
                public Enum Value { get; }

                public struct TransitionInfo
                {
                    public StateInfo Info;
                    public int ID;
                }
                private readonly List<TransitionInfo> m_Done = new List<TransitionInfo>();
                private readonly List<TransitionInfo> m_Error = new List<TransitionInfo>();

                public StateInfo(Enum value)
                {
                    Value = value;
                    ID = GetID(value);
                }

                public void AddTransition<Transition>(StateInfo info, Result result)
                    where Transition : unmanaged, ILogicTransition
                {
                    var id = LogicSystem.Instance.RegJob<Transition>();
                    var item = new TransitionInfo() { Info = info, ID = id };
                    switch (result)
                    {
                        case Result.Done: m_Done.Add(item); break;
                        case Result.Error: m_Error.Add(item); break;
                    }
                }

                public void AddTransition(StateInfo info, Result result)
                {
                    var item = new TransitionInfo() { Info = info, ID = -1 };
                    switch (result)
                    {
                        case Result.Done: m_Done.Add(item); break;
                        case Result.Error: m_Error.Add(item); break;
                    }
                }

                public IEnumerable<TransitionInfo> GetTransitions(Result result)
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