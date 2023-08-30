using System;
using Unity.Entities;
using Common.Defs;
using Common.Core;

using Game.Core;
using Game.Core.Saves;

namespace Game.Model.Weapons
{
    using static Game.Model.Logics.Logic;

    /// <summary>
    /// Реализация оружия
    /// </summary>
    [Serializable, Saved]
    public partial struct Weapon: IPart, IDefinable, IComponentData, IDefineableCallback, IStateData
    {
        private readonly RefLink<WeaponDef> m_RefLink;
        public WeaponDef Def => m_RefLink.Value;

        public int Count;

        public float Time;

        public ObjectID BulletID;

        public Weapon(RefLink<WeaponDef> config)
        {
            m_RefLink = config;
            Time = 0;
            Count = 0;
            BulletID = m_RefLink.Value.Bullet.ID;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddComponentData(entity, new Target());
            context.AddComponentData(entity, new Target.Query());
            Count = Def.ClipSize;
        }
        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion

        /// <summary>
        /// Состояние оружия
        /// </summary>
        [EnumHandle]
        public enum Action
        {
            Attack,
            Shoot,
            Reload,
            Sleep,
        }

        [EnumHandle]
        public enum State
        {
            Active,
            HasAmo,
            Shooting,
        }

        /// <summary>
        /// Список статов оружия
        /// </summary>
        [EnumHandle]
        public enum Stats
        {
            /// <summary>
            /// Урон
            /// </summary>
            Damage,
            /// <summary>
            /// Размер обоймы
            /// </summary>
            ClipSize,
            /// <summary>
            /// Период стрельбы
            /// </summary>
            Rate,
            /// <summary>
            /// Время перезарядки
            /// </summary>
            ReloadTime,
            /// <summary>
            /// Дальность
            /// </summary>
            Range,
        }

        [Serializable]
        public class WeaponDef : IDef<Weapon>
        {
            public BulletConfig Bullet;
            public float Health = 5;
            public float Range = 25;
            public float DamageValue = 1;
            public int BarrelCount;
            public int ClipSize;
            public float Rate = 1;
            public float ReloadTime = 5;
        }
    }
}
