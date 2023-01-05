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
        public Def<UnitDef> Def { get; }

        public Unit(Def<UnitDef> config)
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
                
                if (Def.Value.Weapon is IConfig config)
                {
                    config.Configurate(weapon, context);
                    //TODO: перенести в weapon!
                    context.AddComponentData<Part>(weapon, new Part { Unit = entity });
                }
            }
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
        public class UnitDef : IDef<Unit>
        {
            public WeaponConfig Weapon;

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Speed;

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Health;
        }
    }
}