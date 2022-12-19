using System;
using Common.Defs;
using Unity.Entities;
using Unity.Serialization;
using Unity.Properties;

namespace Game.Model
{
    using Weapons;
    using Stats;


    [ChunkSerializable]
    public unsafe struct BulletTest : IModifier, IDefineable, IComponentData, IEnableableComponent
    {
        private readonly Def<Config> m_Config;

        [DontSerialize]
        private int m_Mod_id;

        public bool Enable;

        static BulletTest()
        {
            ModifierSystem.Registry<BulletTest>();
        }

        public BulletTest(Def<Config> config)
        {
            m_Config = config;
            m_Mod_id = -1;
            Enable = false;
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

        [Serializable]
        public class Config : IDef<BulletTest>
        {
            public float Multiplier;
        }
    }
}
