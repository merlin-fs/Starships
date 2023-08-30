using System;
using Unity.Entities;
using Unity.Properties;
using Unity.Collections;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Common.Core;

    using Core.Repositories;
    using Stats;

    public readonly partial struct WeaponAspect: IAspect
    {
        private readonly Entity m_Self;
        readonly RefRO<Root> m_Root;
        readonly RefRW<Weapon> m_Weapon;
        readonly RefRO<Target> m_Target;
        readonly RefRO<Target.Query> m_TargetQuery;
        [Optional] readonly RefRO<Bullet> m_Bullet;
        [ReadOnly] readonly DynamicBuffer<Stat> m_Stats;
        public Entity Self => m_Self;

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
        public Target Target { get => m_Target.ValueRO; }
        [CreateProperty]
        public Target.Query TargetQuery { get => m_TargetQuery.ValueRO; }

        public Bullet Bullet => m_Bullet.ValueRO;
        public Stat Stat<T>(T stat) where T: struct, IConvertible => m_Stats.GetRO(stat);
        
        private readonly ObjectRepository ObjectRepository => default(DiContext.Var<ObjectRepository>).Value;

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
            Damage.Apply(Root, Target, m_Bullet.ValueRO, Stat(Weapon.Stats.Damage).Value);
        }

        public bool Reload(IDefineableContext context, int count)
        {
            UnityEngine.Debug.Log($"{Self} [Weapon] reload");
            if (m_Bullet.IsValid)
                m_Bullet.ValueRO.Def.RemoveComponentData(m_Self, m_Bullet.ValueRO, context);

            var bulletConfig = ObjectRepository.FindByID(m_Weapon.ValueRO.BulletID);

            if (bulletConfig == null)
                return false;

            bulletConfig.Configurate(m_Self, context);
            m_Weapon.ValueRW.Count = count;
            return count > 0;
        }
    }
}
