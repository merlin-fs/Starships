using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Stats;

    /// <summary>
    /// Реализация оружия
    /// </summary>
    [Serializable]
    public struct Weapon: IPart, IDefineable, IComponentData, IDefineableCallback
    {
        private readonly Def<WeaponConfig> m_Config;
        public WeaponConfig Config => m_Config.Value;

        public int Count;

        public float Time;

        public Weapon(Def<WeaponConfig> config)
        {
            m_Config = config;
            Time = 0;
            Count = 0;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            context.SetName(entity, GetType().Name);

            context.AddBuffer<Modifier>(entity);
            var buff = context.AddBuffer<Stat>(entity);
            Stat.AddStat(buff, GlobalStat.Health, 10);
            Stat.AddStat(buff, Stats.Rate, m_Config.Value.Rate);
            Stat.AddStat(buff, Stats.Damage, m_Config.Value.DamageValue);
            Stat.AddStat(buff, Stats.ReloadTime, m_Config.Value.ReloadTime);
            Stat.AddStat(buff, Stats.ClipSize, m_Config.Value.ClipSize);
            Count = m_Config.Value.ClipSize;
            context.AddComponentData(entity, new Target());
        }

        public void RemoveComponentData(Entity entity, IDefineableContext context)
        {

        }
        #endregion
        /// <summary>
        /// Состояние оружия
        /// </summary>
        public enum State
        {
            NoTarget,
            Shooting,
            Reload,
            Sleep,
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
            public StatValue DamageValue = 1;
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
            public StatValue Rate = 1;
            /// <summary>
            /// Время перезарядки оружия
            /// </summary>
            public StatValue ReloadTime = 5;
        }
    }
}
