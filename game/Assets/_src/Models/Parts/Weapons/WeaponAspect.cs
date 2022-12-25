using System;
using Unity.Entities;
using Unity.Properties;
using Common.Defs;

namespace Game.Model.Weapons
{
    public readonly partial struct WeaponAspect: IAspect
    {
        public readonly Entity Self;

        readonly RefRW<Weapon> m_Weapon;

        [Optional] readonly RefRW<Bullet> m_Bullet;

        [CreateProperty]
        public Bullet Bullet => m_Bullet.IsValid ? m_Bullet.ValueRO : default;
        
        public Weapon.WeaponConfig Config => m_Weapon.ValueRO.Config;

        public int Count => m_Weapon.ValueRO.Count;

        public float Time
        {
            get => m_Weapon.ValueRO.Time;
            set => m_Weapon.ValueRW.Time = value;
        }

        public void Shot()
        {
            m_Weapon.ValueRW.Count -= Config.BarrelCount;
        }

        public void Reload(IDefineableContext context)
        {
            if (m_Bullet.IsValid)
                Config.Bullet.Value.RemoveComponentData(Self, context, m_Bullet.ValueRO);
            
            Config.Bullet.Value.AddComponentData(Self, context);
            m_Weapon.ValueRW.Count = Config.ClipSize;
        }
    }
}
