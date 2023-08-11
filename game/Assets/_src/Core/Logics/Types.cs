using System;

using Game.Core;

using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public interface IPartLogic : ISystem
        {
        }

        public interface ILogic
        {
            void Init(LogicDef def);
        }

        public struct States: IDisposable
        {
            private NativeHashMap<EnumHandle, bool> m_States;

            public States(AllocatorManager.AllocatorHandle allocator)
            {
                m_States = new NativeHashMap<EnumHandle, bool>(1, allocator);
            }

            public NativeHashMap<EnumHandle, bool>.ReadOnly GetReadOnly()
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

            public bool Has(EnumHandle state, bool value)
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

            public States SetState<T>(T state, bool value)
                where T: struct, IConvertible
            {
                return SetState(EnumHandle.FromEnum(state), value);
            }

            public States SetState(EnumHandle state, bool value)
            {
                m_States[state] = value;
                return this;
            }

            public States RemoveState(EnumHandle state)
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
    }
}