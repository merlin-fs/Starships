using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Model
{
    using Stats;

    public class DamageHealth : Damage
    {
        public override void Apply(ref StatAspect stat, float value, IDefineableContext context)
        {
            stat.GetRW(Global.Stat.Health).Damage(value);
        }
    }
}   