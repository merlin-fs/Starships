using System;
using Common.Defs;
using Unity.Entities;
using Unity.Serialization;
using Unity.Properties;

namespace Game.Model.Weapons
{
    using Stats;

    public unsafe struct Bullet: IModifier, IDefineable, IComponentData, IDefineableCallback
    {
        [DontSerialize]
        private readonly Def<Config> m_Config;

        private int m_Mod_id;

        public Bullet(Def<Config> config)
        {
            m_Config = config;
            m_Mod_id = -1;
        }

        [CreateProperty] 
        public float Multiplier => m_Config.Value.Multiplier;
        #region IModifier
        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            stat.Value *= Multiplier;
        }

        public void Attach(Entity entity)
        {
            Modifier.AddModifier(entity, this, Weapon.Stats.Damage, out m_Mod_id);
        }

        public void Dettach(Entity entity)
        {
            Modifier.DelModifier(entity, m_Mod_id);
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
