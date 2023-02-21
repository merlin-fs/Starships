using System;
using Unity.Entities;
using Common.Defs;
using Common.Core;

namespace Game.Model.Weapons
{
    using static Game.Model.Logics.Logic;

    /// <summary>
    /// Реализация оружия
    /// </summary>
    [Serializable]
    public partial struct Weapon: IPart, IDefineable, IComponentData, IDefineableCallback, IStateData
    {
        private readonly Def<WeaponDef> m_Def;
        public WeaponDef Def => m_Def.Value;

        public int Count;

        public float Time;

        public ObjectID BulletID;

        public Weapon(Def<WeaponDef> config)
        {
            m_Def = config;
            Time = 0;
            Count = 0;
            BulletID = m_Def.Value.Bullet.ID;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            //context.SetName(entity, GetType().Name);
            context.AddComponentData(entity, new Target());
            Count = Def.ClipSize;
        }
        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion

        /// <summary>
        /// Состояние оружия
        /// </summary>
        public enum Action
        {
            Init,
            Shooting,
            Shoot,
            Reload,
            Sleep,
        }

        public enum State
        {
            Active,
            NoAmmo,
            HasAmo,
        }

        /// <summary>
        /// Список статов оружия
        /// </summary>
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
