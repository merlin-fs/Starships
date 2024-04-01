using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Common.Defs;

using Game.Core;
using Game.Model.Logics;
using Game.Model.Stats;
using Game.Model.Worlds;

using UnityEngine.Serialization;

namespace Game.Model.Units
{
    [Serializable]
    public partial struct Unit : IUnit, IDefinable, IComponentData, IDefinableCallback, Logic.IStateData
    {
        public RefLink<UnitDef> RefLink { get; }

        public Unit(RefLink<UnitDef> config)
        {
            RefLink = config;
        }
        
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefinableContext context)
        {
            context.AddComponentData(entity, new Map.Placement(RefLink<Map.IPlacement>.Copy(RefLink)));
            context.AddComponentData(entity, new Map.Transform());
            context.AddComponentData(entity, new Map.Target());
            context.AddComponentData(entity, new Move());
        }

        public void RemoveComponentData(Entity entity, IDefinableContext context) { }
        #endregion
        
        [EnumHandle]
        public enum Action
        {
            WeaponsActivate,
            Attack,
        }

        [EnumHandle]
        public enum State
        {
            Stop,
            WeaponInRange,
        }

        [EnumHandle]
        public enum Stats
        {
            Speed,
        }

        [Serializable]
        public class UnitDef : IDef<Unit>, Map.IPlacement
        {
            [SerializeField] int2 size = new int2(1, 1);
            [SerializeField] float3 pivot = new float3(0, 0f, 0f);
            [SerializeField, SelectType(typeof(Map.Layers.ILayer))] string layer;
            #region Map.IPlacement
            int2 Map.IPlacement.Size => size;
            float3 Map.IPlacement.Pivot => pivot;
            TypeIndex Map.IPlacement.Layer => TypeManager.GetTypeIndex(Type.GetType(layer));
            #endregion
            public List<ChildConfig> Parts = new List<ChildConfig>();

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Speed;

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Health;
        }
    }
}