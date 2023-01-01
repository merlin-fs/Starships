using System;
using Unity.Entities;
using Unity.Properties;
using Common.Defs;
using Game.Core.Repositories;

namespace Game.Model.Weapons
{
    public readonly partial struct WeaponAspect: IAspect
    {
        private readonly Entity m_Self;
        public Entity Self => m_Self;

        readonly RefRW<Weapon> m_Weapon;
        [Optional] readonly RefRW<Bullet> m_Bullet;
        [Optional] readonly RefRO<Part> m_Part;
        readonly RefRW<Target> m_Target;
        public Weapon.WeaponConfig Config => m_Weapon.ValueRO.Config;

        [CreateProperty]
        public string Bullet => Config.Bullet.name;

        [CreateProperty]
        public int Count => m_Weapon.ValueRO.Count;

        [CreateProperty]
        public Entity Unit => m_Part.IsValid ? m_Part.ValueRO.Unit : default;

        [CreateProperty]
        public Entity Target => m_Target.ValueRO.Value;

        [CreateProperty]
        public uint SoughtTeams => m_Target.ValueRO.SoughtTeams;

        public float Time
        {
            get => m_Weapon.ValueRO.Time;
            set => m_Weapon.ValueRW.Time = value;
        }

        public void Shot(IDefineableContext context)
        {
            m_Weapon.ValueRW.Count -= Config.BarrelCount;
            if (m_Weapon.ValueRW.Count < 0)
                m_Weapon.ValueRW.Count = 0;
        }

        public bool Reload(IDefineableContext context)
        {
            if (m_Bullet.IsValid)
                m_Bullet.ValueRO.Def.RemoveComponentData(m_Self, context, m_Bullet.ValueRO);

            var bullet = Repositories.Instance.ConfigsAsync().Result
                .FindByID(m_Weapon.ValueRO.BulletID);
            if (bullet == null)
                return false;

            bullet.Configurate(m_Self, context);
            m_Weapon.ValueRW.Count = Config.ClipSize;
            return true;
        }
        public void SetSoughtTeams(uint value) => m_Target.ValueRW.SoughtTeams = value;
    }
}
