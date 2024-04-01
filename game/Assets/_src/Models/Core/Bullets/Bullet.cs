using System;
using Common.Defs;
using Unity.Entities;
using Unity.Serialization;
using Unity.Properties;

namespace Game.Model.Weapons
{
    using Stats;

    [Serializable]
    public struct Bullet: IModifier, IDefinable, IComponentData, IDefinableCallback
    {
        private readonly RefLink<BulletDef> m_Config;

        private ulong m_ModUID;

        public BulletDef Def => m_Config.Value;

        [CreateProperty] public float Multiplier => m_Config.Value.Multiplier;
        [CreateProperty] public float Range => m_Config.Value.Range;

        public Bullet(RefLink<BulletDef> config)
        {
            m_Config = config;
            m_ModUID = 0;
        }
        #region IModifier
        public void Estimation(Entity entity, ref Stat stat, float delta)
        {
            stat.ModMull(Multiplier);
        }

        public void Attach(Entity entity)
        {
            UnityEngine.Debug.Log($"{entity} [Bullet] AddModifier");
            m_ModUID = Modifier.AddModifierAsync(entity, ref this, Weapon.Stats.Damage);
        }

        public void Dettach(Entity entity)
        {
            UnityEngine.Debug.Log($"{entity} [Bullet] DelModifierAsync");
            Modifier.DelModifierAsync(entity, m_ModUID);
        }
        #endregion
        #region IDefineableCallback
        void IDefinableCallback.AddComponentData(Entity entity, IDefinableContext context)
        {
            Attach(entity);
        }

        void IDefinableCallback.RemoveComponentData(Entity entity, IDefinableContext context)
        {
            Dettach(entity);
        }
        #endregion
        
        [Serializable]
        public class BulletDef : IDef<Bullet>
        {
            public DamageConfig DamageType;
            public float Multiplier;
            public float Range;
        }
    }
}
