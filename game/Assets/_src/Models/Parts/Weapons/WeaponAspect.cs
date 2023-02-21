using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Collections;
using Common.Defs;
using Unity.Transforms;

namespace Game.Model.Weapons
{
    using Core.Repositories;
    using Stats;

    public readonly partial struct WeaponAspect: IAspect
    {
        private readonly Entity m_Self;
        readonly RefRO<Root> m_Root;
        readonly RefRW<Weapon> m_Weapon;
        readonly RefRW<Target> m_Target;
        [Optional] readonly RefRO<Bullet> m_Bullet;
        [ReadOnly] readonly DynamicBuffer<Stat> m_Stats;
        readonly TransformAspect m_Transform;
        public Entity Self => m_Self;
        public TransformAspect Transform => m_Transform;

        #region DesignTime

#if UNITY_EDITOR
        [CreateProperty]
        public string BulletName => m_Weapon.ValueRO.BulletID.ToString();
#endif
        #endregion
        public Weapon.WeaponDef Config => m_Weapon.ValueRO.Def;

        [CreateProperty]
        public int Count => m_Weapon.ValueRO.Count;

        [CreateProperty]
        public Entity Root => m_Root.ValueRO.Value;

        [CreateProperty]
        public Target Target { get => m_Target.ValueRO; set => m_Target.ValueRW = value; }

        [CreateProperty]
        public uint SoughtTeams => m_Target.ValueRO.SoughtTeams;

        public Bullet Bullet => m_Bullet.ValueRO;
        public Stat Stat(Enum stat) => m_Stats.GetRO(stat);

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
            //DamageSystem.Damage(Self, m_Transform.ValueRO, Target, m_Bullet.ValueRO, Stat(Weapon.Stats.Damage).Value, context);
            Damage.Apply(Root, Transform.WorldTransform, Target, m_Bullet.ValueRO, Stat(Weapon.Stats.Damage).Value);
        }

        public bool Reload(IDefineableContext context, int count)
        {
            UnityEngine.Debug.Log($"{Self} [Weapon] reload");
            if (m_Bullet.IsValid)
                m_Bullet.ValueRO.Def.RemoveComponentData(m_Self, context, m_Bullet.ValueRO);

            var bulletConfig = Repositories.Instance.ConfigsAsync().Result
                .FindByID(m_Weapon.ValueRO.BulletID);

            if (bulletConfig == null)
                return false;

            bulletConfig.Configurate(m_Self, context);
            m_Weapon.ValueRW.Count = count;
            return count > 0;
        }
    }
}
