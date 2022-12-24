﻿using System;
using Unity.Entities;
using Unity.Properties;
using Common.Defs;

public enum GlobalStat
{
    Health,
}

namespace Game.Model.Weapons
{
    using Stats;

    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

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
            m_Weapon.ValueRW.Count--;
        }

        public void Reload(IDefineableContext context)
        {
            if (m_Bullet.IsValid)
                Config.Bullet.Value.RemoveComponentData(Self, context, m_Bullet.ValueRO);
            
            Config.Bullet.Value.AddComponentData(Self, context);
            m_Weapon.ValueRW.Count = Config.ClipSize;
        }
    }

    /// <summary>
    /// Реализация оружия
    /// </summary>
    [ChunkSerializable]
    public unsafe struct Weapon: IPart, IDefineable, IComponentData, IDefineableCallback
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
            var buff = context.AddBuffer<Stat>(entity);
            Stat.AddStat(buff, Stats.Rate, m_Config.Value.Rate);
            Stat.AddStat(buff, Stats.Damage, m_Config.Value.DamageValue);
            Stat.AddStat(buff, Stats.ReloadTime, m_Config.Value.ReloadTime);
            Stat.AddStat(buff, Stats.ClipSize, m_Config.Value.ClipSize);
            Stat.AddStat(buff, GlobalStat.Health, 10);

            m_Config.Value.Bullet.Value.AddComponentData(entity, context);
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
