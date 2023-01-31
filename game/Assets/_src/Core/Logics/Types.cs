using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public struct States: IDisposable
        {
            private NativeHashMap<LogicHandle, bool> m_States;

            public States(AllocatorManager.AllocatorHandle allocator)
            {
                m_States = new NativeHashMap<LogicHandle, bool>(1, allocator);
            }

            public NativeHashMap<LogicHandle, bool> .ReadOnly GetReadOnly()
            {
                return m_States.AsReadOnly();
            }

            public bool Any(States states)
            {
                var (src, dst) = states.m_States.Count > m_States.Count
                    ? (m_States, states.m_States)
                    : (states.m_States, m_States);
                foreach(var iter in src)
                {
                    if (dst.TryGetValue(iter.Key, out bool value) && iter.Value == value)
                        return true;
                }
                return false;
            }

            public bool All(States states)
            {
                foreach (var iter in m_States)
                {
                    if (!states.m_States.TryGetValue(iter.Key, out bool value) || iter.Value != value)
                        return false;
                }
                return true;
            }

            public bool Has(LogicHandle state, bool value)
            {
                return m_States.TryGetValue(state, out bool stateValue) && stateValue == value;
            }

            public States AddIntersect(States added, States compares)
            {
                foreach (var iter in added.m_States)
                    if (!compares.m_States.TryGetValue(iter.Key, out bool value) || iter.Value != value)
                        m_States.Add(iter.Key, iter.Value);
                return this;
            }

            public States SetState(States states)
            {
                foreach (var iter in states.m_States)
                    m_States[iter.Key] = iter.Value;
                return this;
            }

            public States SetState(Enum state, bool value)
            {
                return SetState(LogicHandle.FromEnum(state), value);
            }

            public States SetState(LogicHandle state, bool value)
            {
                m_States[state] = value;
                return this;
            }

            public States RemoveState(LogicHandle state)
            {
                m_States.Remove(state);
                return this;
            }

            public States RemoveState(States states)
            {
                foreach (var iter in states.m_States)
                    m_States.Remove(iter.Key);
                return this;
            }

            public void Dispose()
            {
                m_States.Dispose();
            }
        }

        public struct Action : IBufferElementData
        {
            public LogicHandle Value;
        }

        public struct WorldState : IBufferElementData
        {
            public LogicHandle ID;
            public bool Value;
        }
    }
}