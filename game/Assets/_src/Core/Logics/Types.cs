using System;

using Game.Core;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public interface ISlice
        {
            bool IsConditionHit(ref SliceContext context);
            void Execute(ref SliceContext context);
        }

        public unsafe readonly ref struct SliceContext
        {
            private readonly void* m_Logic;
            private readonly void* m_Lookup;
            private readonly void* m_Writer;
            private readonly int m_Index;
            private readonly void* m_Children;
            

            public ref Aspect Logic => ref UnsafeUtility.AsRef<Aspect>(m_Logic);
            public ref Aspect.Lookup Lookup => ref UnsafeUtility.AsRef<Aspect.Lookup>(m_Lookup);
            public ref EntityCommandBuffer.ParallelWriter Writer => ref UnsafeUtility.AsRef<EntityCommandBuffer.ParallelWriter>(m_Writer);
            public int Index => m_Index;
            public ref BufferLookup<ChildEntity> Children => ref UnsafeUtility.AsRef<BufferLookup<ChildEntity>>(m_Children);
            
            public SliceContext(int idx, ref Aspect logic, ref Aspect.Lookup lookup, ref BufferLookup<ChildEntity> children,
                ref EntityCommandBuffer.ParallelWriter writer)
            {
                m_Logic = UnsafeUtility.AddressOf(ref logic);
                m_Lookup = UnsafeUtility.AddressOf(ref lookup);
                m_Writer = UnsafeUtility.AddressOf(ref writer);
                m_Children = UnsafeUtility.AddressOf(ref children);
                m_Index = idx;
            }
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