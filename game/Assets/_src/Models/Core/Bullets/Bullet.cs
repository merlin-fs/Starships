using System;
using Common.Defs;
using Unity.Entities;
using Unity.Serialization;
using Unity.Properties;

namespace Game.Model.Weapons
{
    using Stats;
    using static UnityEditor.Progress;

    public unsafe struct Bullet: IModifier, IDefineable, IComponentData, IDefineableCallback
    {
        [DontSerialize]
        private readonly Def<Config> m_Config;

        private int m_ModUid;

        public Bullet(Def<Config> config)
        {
            m_Config = config;
            m_ModUid = -1;
        }

        [CreateProperty] 
        public float Multiplier => m_Config.Value.Multiplier;
        #region IModifier
        public void Estimation(Entity entity, ref StatValue stat, float delta)
        {
            UnityEngine.Debug.Log($"Estimation {entity}");
            //stat.Value *= Multiplier;
            UnityEngine.Debug.Log($"Estimation done");
        }

        public void Attach(Entity entity)
        {
            m_ModUid = Modifier.AddModifier(entity, ref this, Weapon.Stats.Damage);
        }

        public void Dettach(Entity entity)
        {
            Modifier.DelModifier(entity, m_ModUid);
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
