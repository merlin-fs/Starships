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
            public CustomHandle LogicHandle => CustomHandle.From<Unit>();

            private readonly void* m_LogicLookup;
            private readonly void* m_Aspect;
            private readonly void* m_LookupMapTransform;
            private readonly void* m_Children;
            
            private readonly int m_Idx;
            public Logic.Aspect Logic => LogicLookup[Aspect.Self];
            public Aspect Aspect => UnsafeUtility.AsRef<Aspect>(m_Aspect);
            public Logic.Aspect.Lookup LogicLookup => UnsafeUtility.AsRef<Logic.Aspect.Lookup>(m_LogicLookup);
            public BufferLookup<ChildEntity> Children => UnsafeUtility.AsRef<BufferLookup<ChildEntity>>(m_Children);
            public ComponentLookup<Map.Transform> LookupMapTransform => UnsafeUtility.AsRef<ComponentLookup<Map.Transform>>(m_LookupMapTransform);

            public Context(int idx, ref Logic.Aspect.Lookup logicLookup, ref Aspect aspect, 
                ref ComponentLookup<Map.Transform> lookupMapTransform, ref BufferLookup<ChildEntity> children): 
                this(idx, UnsafeUtility.AddressOf(ref logicLookup), UnsafeUtility.AddressOf(ref aspect), 
                    UnsafeUtility.AddressOf(ref lookupMapTransform), UnsafeUtility.AddressOf(ref children))
            {
            }

            private Context(int idx, void* logicLookup, void* aspect, void* lookupMapTransform, void* children)
            {
                m_Idx = idx;
                m_LogicLookup = logicLookup;
                m_Aspect = aspect;
                m_LookupMapTransform = lookupMapTransform;
                m_Children = children;
            }
        }
    }
}