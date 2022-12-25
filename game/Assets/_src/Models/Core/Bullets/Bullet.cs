using System;
using Common.Defs;
using Unity.Entities;
using Unity.Serialization;
using Unity.Properties;

namespace Game.Model.Weapons
{
    using Stats;

    public struct Bullet: IModifier, IDefineable, IComponentData, IDefineableCallback
    {
        [DontSerialize]
        private readonly Def<Config> m_Config;

        private ulong m_ModUID;

        public float MultiplierInner;

        public Bullet(Def<Config> config)
        {
            m_Config = config;
            m_ModUID = 0;
            MultiplierInner = m_Config.Value.Multiplier;
        }

        [CreateProperty] 
        public float Multiplier => m_Config.Value.Multiplier;
        #region IModifier
        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            stat.Mull(MultiplierInner);
        }

        public void Attach(Entity entity)
        {
            m_ModUID = Modifier.AddModifierAsync(entity, ref this, Weapon.Stats.Damage);
        }

        public void Dettach(Entity entity)
        {
            Modifier.DelModifierAsync(entity, m_ModUID);
        }
        #endregion
        #region IDefineableCallback
        void IDefineableCallback.AddComponentData(Entity entity, IDefineableContext context)
        {
            Attach(entity);
        }

        void IDefineableCallback.RemoveComponentData(Entity entity, IDefineableContext context)
        {
            Dettach(entity);
        }
        #endregion
        
        [Serializable]
        public class Config : IDef<Bullet>
        {
            public float Multiplier;
        }
    }
}
