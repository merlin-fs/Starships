using System;
using Common.Defs;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    public enum GlobalStat
    {
        Health,
    }
}

namespace Game.Model.Units
{
    using Weapons;
    using Stats;

    /// <summary>
    /// Реализация юнита (корабля)
    /// </summary>
    [Serializable]
    public struct Unit : IUnit, IDefineable, IComponentData, IDefineableCallback
    {
        public Def<UnitConfig> Def { get; }

        public Unit(Def<UnitConfig> config)
        {
            Def = config;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddBuffer<Modifier>(entity);

            var buff = context.AddBuffer<Stat>(entity);
            Stat.AddStat(buff, GlobalStat.Health, Def.Value.Speed);
            Stat.AddStat(buff, Stats.Speed, Def.Value.Speed);

            context.SetName(entity, GetType().Name);

            var weapon = context.FindEntity(Def.Value.Weapon.PrefabID);
            Def.Value.Weapon.Value.AddComponentData(weapon, context);
            Def.Value.Weapon.Logic.AddComponentData(weapon, context);
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