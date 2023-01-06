using System;
using Common.Defs;
using Unity.Entities;
using UnityEngine;

namespace Game.Model.Units
{
    using Weapons;
    using Stats;
    using System.Collections.Generic;
    using Game.Core.Defs;

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
            //context.SetName(entity, GetType().Name);
            /*
            foreach (var iter in Def.Value.Parts)
            {
                var part = context.FindEntity(iter.Child);
                if (part == Entity.Null)
                    part = entity;

                if (iter is IConfig config)
                {
                    config.Configurate(part, context);
                    //TODO: перенести в weapon!
                    context.AddComponentData<Part>(part, new Part { Unit = entity });
                }
            }
            */
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
            public List<ChildConfig> Parts = new List<ChildConfig>();

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Speed;

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Health;
        }
    }
}