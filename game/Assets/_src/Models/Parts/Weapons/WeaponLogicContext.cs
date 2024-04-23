using System;
using Game.Core;
using Unity.Collections.LowLevel.Unsafe;
using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public readonly struct Context: Logic.ILogicContext<Weapon>
        {
            public LogicHandle LogicHandle => LogicHandle.From<Weapon>();
            public Logic.Aspect Logic => LogicLookup[Aspect.Self];
            public Logic.Aspect.Lookup LogicLookup { get; }
            public int Idx { get; }
            public WeaponAspect Aspect { get; }
            public float Delta { get; }

            public Context(int idx, ref Logic.Aspect.Lookup logicLookup, ref WeaponAspect aspect, float delta) 
            {
                Idx = idx;
                LogicLookup = logicLookup;
                Aspect = aspect;
                Delta = delta;
            }
        }
    }
}