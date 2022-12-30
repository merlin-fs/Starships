using System;
using Unity.Entities;
using Unity.Properties;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Units;

    public readonly partial struct WeaponAspect: IAspect
    {
        public readonly Entity Self;
        readonly RefRW<Weapon> m_Weapon;
        [Optional] readonly RefRW<Bullet> m_Bullet;
        [Optional] readonly RefRO<Part> m_Part;
        readonly RefRW<Target> m_Target;

        [CreateProperty]
        public string Bullet => Config.Bullet.name;

        [CreateProperty]
        public int Count => m_Weapon.ValueRO.Count;

        [CreateProperty]
        public Entity Unit => m_Part.IsValid ? m_Part.ValueRO.Unit : default;

        [CreateProperty]
        public Entity Target => m_Target.ValueRO.Value;

        [CreateProperty]
        public uint SoughtTeams {
            get => m_Target.ValueRO.SoughtTeams;
            set => m_Target.ValueRW.SoughtTeams = value;
        }

        public Weapon.WeaponConfig Config => m_Weapon.ValueRO.Config;


        public float Time
        {
            get => m_Weapon.ValueRO.Time;
            set => m_Weapon.ValueRW.Time = value;
        }

        public void Shot()
        {
            m_Weapon.ValueRW.Count -= Config.BarrelCount;
            if (m_Weapon.ValueRW.Count < 0)
                m_Weapon.ValueRW.Count = 0;
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
