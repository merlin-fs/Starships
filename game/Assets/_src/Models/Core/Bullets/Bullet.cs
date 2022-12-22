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

        private int m_ModIndex;

        public float MultiplierInner;

        public Bullet(Def<Config> config)
        {
            m_Config = config;
            m_ModIndex = 0;
            MultiplierInner = m_Config.Value.Multiplier;
        }

        [CreateProperty] 
        public float Multiplier => m_Config.Value.Multiplier;
        #region IModifier
        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            stat.Value *= MultiplierInner;
        }

        public async void Attach(Entity entity)
        {
            m_ModIndex = await Modifier.AddModifierAsync(entity, ref this, Weapon.Stats.Damage);
        }

        public async void Dettach(Entity entity)
        {
            await Modifier.DelModifierAsync(entity, m_ModIndex);
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

            public void Estimation(Entity entity, ref StatValue stat, float delta)
            {
                stat.Value *= Multiplier;
            }
        }
    }
}
