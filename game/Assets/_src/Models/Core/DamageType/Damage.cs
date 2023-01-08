using System;
using Common.Defs;

using Game.Model.Logics;
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
        void Apply(ref DynamicBuffer<Stat> stats, float value, IDefineableContext context);
    }

    [Serializable]
    public abstract class Damage: IDamage
    {
        public abstract void Apply(ref DynamicBuffer<Stat> stats, float value, IDefineableContext context);
    }
}