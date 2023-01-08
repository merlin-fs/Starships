using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Model
{
    using Stats;

    public class DamageHealth : Damage
    {
        public override DamageTargets Targets => DamageTargets.One;
        public override void Apply(ref DynamicBuffer<Stat> stats, float value, IDefineableContext context)
        {
            stats.GetRW(GlobalStat.Health).Damage(value);
        }
    }
}   