using System;
using Common.Defs;

namespace Game.Model
{
    using Stats;

    public enum DamageTargets
    {
        One,
        AoE,
    }

    public interface IDamage
    {
        void Apply(ref StatAspect stat, float value, IDefineableContext context);
    }

    [Serializable]
    public abstract class Damage: IDamage
    {
        public abstract void Apply(ref StatAspect stat, float value, IDefineableContext context);
    }
}