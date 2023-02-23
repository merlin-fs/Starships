﻿using System;
using Common.Defs;
using Unity.Entities;
using UnityEngine;

namespace Game.Model.Units
{
    using Weapons;
    using Stats;
    using System.Collections.Generic;
    using Game.Core.Defs;
    using static Game.Model.Logics.Logic;

    /// <summary>
    /// Реализация юнита (корабля)
    /// </summary>
    [Serializable]
    public struct Unit : IUnit, IDefinable, IComponentData, IDefineableCallback, IStateData
    {
        public Def<UnitDef> Def { get; }

        public Unit(Def<UnitDef> config)
        {
            Def = config;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
        }

        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion
        public enum Action
        {
            ActiveWeapons,
        }

        public enum State
        {
            Stop,
            WeaponsActive,
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