using System;
using Game.Core;
using Unity.Collections.LowLevel.Unsafe;
using Game.Model.Logics;
using Game.Model.Worlds;

using Unity.Entities;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public unsafe readonly struct Context: Logic.ILogicContext<Unit>
        {
            public LogicHandle LogicHandle => LogicHandle.From<Unit>();

            private readonly void* m_LogicLookup;
            private readonly void* m_Aspect;
            private readonly void* m_LookupMapTransform;
            private readonly void* m_Children;
            private readonly void* m_Writer;
            
            private readonly int m_Idx;
            public int SortKey => m_Idx;
            public Logic.Aspect Logic => LogicLookup[Aspect.Self];
            public Aspect Aspect => UnsafeUtility.AsRef<Aspect>(m_Aspect);
            public Logic.Aspect.Lookup LogicLookup => UnsafeUtility.AsRef<Logic.Aspect.Lookup>(m_LogicLookup);
            public BufferLookup<ChildEntity> Children => UnsafeUtility.AsRef<BufferLookup<ChildEntity>>(m_Children);
            public ComponentLookup<Map.Move> LookupMapTransform => UnsafeUtility.AsRef<ComponentLookup<Map.Move>>(m_LookupMapTransform);
            public EntityCommandBuffer.ParallelWriter Writer => UnsafeUtility.AsRef<EntityCommandBuffer.ParallelWriter>(m_Writer);

            public Context(int idx, ref Logic.Aspect.Lookup logicLookup, ref Aspect aspect, 
                ref ComponentLookup<Map.Move> lookupMapTransform, ref BufferLookup<ChildEntity> children, ref EntityCommandBuffer.ParallelWriter writer): 
                this(idx, UnsafeUtility.AddressOf(ref logicLookup), UnsafeUtility.AddressOf(ref aspect), 
                    UnsafeUtility.AddressOf(ref lookupMapTransform), UnsafeUtility.AddressOf(ref children), 
                    UnsafeUtility.AddressOf(ref writer))
            {
            }

            private Context(int idx, void* logicLookup, void* aspect, void* lookupMapTransform, void* children, void* writer)
            {
                m_Idx = idx;
                m_LogicLookup = logicLookup;
                m_Aspect = aspect;
                m_LookupMapTransform = lookupMapTransform;
                m_Children = children;
                m_Writer = writer;
            }
        }
    }
}