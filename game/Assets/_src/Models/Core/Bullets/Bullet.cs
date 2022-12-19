using System;
using Common.Defs;
using Unity.Entities;
using Unity.Serialization;
using Unity.Properties;

namespace Game.Model
{
    using Weapons;
    using Stats;
    using System.Xml;


    [ChunkSerializable]
    public unsafe struct Bullet: IModifier, IDefineable, IComponentData, IDefineableCallback
    {
        private readonly Def<Config> m_Config;

        [DontSerialize]
        private int m_Mod_id;

        static Bullet()
        {
            ModifierSystem.Registry<Bullet>();
        }

        public Bullet(Def<Config> config)
        {
            m_Config = config;
            m_Mod_id = -1;
        }

        [CreateProperty]
        public float Multiplier => m_Config.Value.Multiplier;

        public void Estimation(TimeSpan delta, ref StatValue stat)
        {
            stat.Value *= Multiplier;
        }

        public void Attach(DynamicBuffer<Modifier> buff)
        {
            Modifier.AddModifier(buff, this, Weapon.Stats.Damage, out m_Mod_id);
        }

        public void Dettach(DynamicBuffer<Modifier> buff)
        {
            Modifier.DelModifier(buff, m_Mod_id);
        }

        void IDefineableCallback.AddComponentData(Entity entity, IDefineableContext context)
        {
            var modifiers = World.DefaultGameObjectInjectionWorld.EntityManager.GetAspect<ModifiersAspect>(entity);
            Attach(modifiers.Items);
        }

        void IDefineableCallback.RemoveComponentData(Entity entity, IDefineableContext context)
        {
            var modifiers = World.DefaultGameObjectInjectionWorld.EntityManager.GetAspect<ModifiersAspect>(entity);
            Dettach(modifiers.Items);
        }

        [Serializable]
        public class Config : IDef<Bullet>
        {
            public float Multiplier;
        }
    }
}
