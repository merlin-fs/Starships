using System;
using Unity.Entities;
using Common.Defs;
using Common.Core;

namespace Game.Model.Weapons
{
    using Stats;

    /// <summary>
    /// Реализация оружия
    /// </summary>
    [Serializable]
    public struct Weapon: IPart, IDefineable, IComponentData, IDefineableCallback
    {
        private readonly Def<WeaponConfig> m_Def;
        public WeaponConfig Def => m_Def.Value;

        public int Count;

        public float Time;

        public ObjectID BulletID;

        public Weapon(Def<WeaponConfig> config)
        {
            m_Def = config;
            Time = 0;
            Count = 0;
            BulletID = m_Def.Value.Bullet.ID;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            context.SetName(entity, GetType().Name);

            context.AddComponentData(entity, new Target());
            
            context.AddBuffer<Modifier>(entity);
            var buff = context.AddBuffer<Stat>(entity);

            buff.AddStat(GlobalStat.Health, Def.Health);
            buff.AddStat(Stats.Rate, Def.Rate);
            buff.AddStat(Stats.Damage, Def.DamageValue);
            buff.AddStat(Stats.ReloadTime, Def.ReloadTime);
            buff.AddStat(Stats.ClipSize, Def.ClipSize);

            Count = Def.ClipSize;
        }
        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion
        /// <summary>
        /// Состояние оружия
        /// </summary>
        public enum State
        {
            Shooting,
            Shoot,
            Reload,
            Sleep,
        }
        public enum Result
        { 
            Done,
            NoAmmo,
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
        }

        [Serializable]
        public class WeaponConfig : IDef<Weapon>
        {
            /// <summary>
            /// Тип пуль
            /// </summary>
            public BulletConfig Bullet;
            /// <summary>
            /// Значение урона
            /// </summary>
            public int Health = 5;

            public float DamageValue = 1;
            /// <summary>
            /// Количество стволов
            /// </summary>
            public int BarrelCount;
            /// <summary>
            /// Размер обоймы
            /// </summary>
            public int ClipSize;
            /// <summary>
            /// Период стрельбы
            /// </summary>
            public float Rate = 1;
            /// <summary>
            /// Время перезарядки оружия
            /// </summary>
            public float ReloadTime = 5;
        }
    }
}
