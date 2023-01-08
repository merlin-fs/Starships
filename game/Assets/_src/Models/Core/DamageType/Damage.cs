using System;
using Common.Defs;
using Game.Model.Stats;
using Unity.Entities;

namespace Game.Model
{
    public enum DamageTargets
    {
        One,
        AoE,
    }

    public interface IDamage
    {
        DamageTargets Targets { get; }
        void Apply(ref DynamicBuffer<Stat> stats, float value, IDefineableContext context);
    }


    [Serializable]
    public abstract class Damage: IDamage
    {
        public abstract DamageTargets Targets { get; }
        public abstract void Apply(ref DynamicBuffer<Stat> stats, float value, IDefineableContext context);
    }
}