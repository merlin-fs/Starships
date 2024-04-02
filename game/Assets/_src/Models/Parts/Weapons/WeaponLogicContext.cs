using System;
using Game.Core;
using Unity.Collections.LowLevel.Unsafe;
using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public unsafe readonly struct Context: Logic.ILogicContext<Weapon>
        {
            public CustomHandle LogicHandle => CustomHandle.From<Weapon>();

            private readonly void* m_LogicLookup;
            private readonly void* m_Aspect;
            private readonly int m_Idx;
            public Logic.Aspect Logic => LogicLookup[Aspect.Self];
            public WeaponAspect Aspect => UnsafeUtility.AsRef<WeaponAspect>(m_Aspect);
            public ref Logic.Aspect.Lookup LogicLookup => ref UnsafeUtility.AsRef<Logic.Aspect.Lookup>(m_LogicLookup);

            public Context(int idx, ref Logic.Aspect.Lookup logicLookup, ref WeaponAspect aspect): 
                this(idx, UnsafeUtility.AddressOf(ref logicLookup), UnsafeUtility.AddressOf(ref aspect))
            {
            }

            private Context(int idx, void* logicLookup, void* aspect)
            {
                m_Idx = idx;
                m_LogicLookup = logicLookup;
                m_Aspect = aspect;
            }
        }
    }
}