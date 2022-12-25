using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Model
{
    public enum GlobalStat
    {
        Health,
    }
}

namespace Game.Model.Units
{
    using Game.Model.Weapons;

    using Stats;

    using Unity.Collections;
    using Unity.Transforms;

    using UnityEngine;

    /// <summary>
    /// Реализация юнита (корабля)
    /// </summary>
    [Serializable]
    public unsafe struct Unit : IUnit, IDefineable, IComponentData, IDefineableCallback
    {
        private readonly Def<UnitConfig> m_Config;
        public UnitConfig Config => m_Config.Value;

        public Unit(Def<UnitConfig> config)
        {
            m_Config = config;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddBuffer<Modifier>(entity);

            var buff = context.AddBuffer<Stat>(entity);
            Stat.AddStat(buff, GlobalStat.Health, m_Config.Value.Speed);
            Stat.AddStat(buff, Stats.Speed, m_Config.Value.Speed);

            context.SetName(entity, GetType().Name);

            var weapon = context.CreateEntity();
            Config.Weapon.Value.AddComponentData(weapon, context);
            Config.Weapon.Logic.AddComponentData(weapon, context);
            context.AddComponentData(weapon, new Parent() { Value = entity });
        }

        public void RemoveComponentData(Entity entity, IDefineableContext context)
        {

        }
        #endregion

        public enum Stats
        {
            Speed,
        }

        [Serializable]
        public class UnitConfig : IDef<Unit>
        {
            public WeaponConfig Weapon;

            public StatValue Speed = 1;
        }
    }
}