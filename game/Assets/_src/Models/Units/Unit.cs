using System;
using Common.Defs;
using Unity.Entities;
using UnityEngine;

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
            context.SetName(entity, GetType().Name);

            if (Def.Value.Weapon != null)
            {
                var weapon = context.FindEntity(Def.Value.Weapon.PrefabID);
                if (weapon == Entity.Null)
                    weapon = entity;

                Def.Value.Weapon.Value.AddComponentData(weapon, context);
                Def.Value.Weapon.Logic.AddComponentData(weapon, context);
                context.AddComponentData<Part>(weapon, new Part { Unit = entity });
            }

            context.AddBuffer<Modifier>(entity);
            var buff = context.AddBuffer<Stat>(entity);

            buff.AddStat(GlobalStat.Health, Def.Value.Health.Value);
            buff.AddStat(Stats.Speed, Def.Value.Speed.Value);
        }

        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion
        public enum State
        {
            Stop,
            Destroy,
        }

        public enum Stats
        {
            Speed,
        }

        [Serializable]
        public class UnitConfig : IDef<Unit>
        {
            public WeaponConfig Weapon;

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Speed;

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Health;
        }
    }
}