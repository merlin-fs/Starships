using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Model
{
    using Stats;

    public class DamageExplose : Damage
    {
        public override DamageTargets Targets => DamageTargets.AoE;
        public override void Apply(ref DynamicBuffer<Stat> stats, float value, IDefineableContext context)
        {
            stats.GetRW(GlobalStat.Health).Damage(value);
        }
    }
}   