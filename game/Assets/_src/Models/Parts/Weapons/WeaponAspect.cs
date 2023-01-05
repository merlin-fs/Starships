using System;
using Unity.Entities;
using Unity.Properties;
using Common.Defs;
using Unity.Collections;

namespace Game.Model.Weapons
{
    using Stats;
    using Core.Repositories;

    public readonly partial struct WeaponAspect: IAspect
    {
        private readonly Entity m_Self;
        public Entity Self => m_Self;

        readonly RefRW<Weapon> m_Weapon;
        readonly RefRW<Target> m_Target;

        [Optional] readonly RefRW<Bullet> m_Bullet;
        [Optional] readonly RefRO<Part> m_Part;
        [ReadOnly] readonly DynamicBuffer<Stat> m_Stats;

        public Weapon.WeaponDef Config => m_Weapon.ValueRO.Def;

        [CreateProperty]
        public string Bullet => Config.Bullet.name;

        [CreateProperty]
        public int Count => m_Weapon.ValueRO.Count;

        [CreateProperty]
        public Entity Unit => m_Part.IsValid ? m_Part.ValueRO.Unit : default;

        [CreateProperty]
        public Target Target => m_Target.ValueRO;

        [CreateProperty]
        public uint SoughtTeams => m_Target.ValueRO.SoughtTeams;

        public Stat Stat(Enum stat) => m_Stats.GetRO(stat);

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
