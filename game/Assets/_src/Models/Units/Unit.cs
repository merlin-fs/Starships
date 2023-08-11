using System;
using Common.Defs;

using Game.Core;
using Game.Model.Worlds;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Game.Model.Units
{
    using Weapons;
    using Stats;
    using System.Collections.Generic;
    using Game.Core.Defs;
    using static Game.Model.Logics.Logic;

    [Serializable]
    public struct Unit : IUnit, IDefinable, IComponentData, IDefineableCallback, IStateData
    {
        public RefLink<UnitDef> RefLink { get; }

        public Unit(RefLink<UnitDef> config)
        {
            RefLink = config;
        }
        #region IDefineableCallback
        public void AddComponentData(Entity entity, IDefineableContext context)
        {
            context.AddComponentData(entity, new Map.Placement(RefLink<Map.IPlacement>.Copy(RefLink)));
        }

        public void RemoveComponentData(Entity entity, IDefineableContext context) { }
        #endregion
        
        [EnumHandle]
        public enum Action
        {
            ActiveWeapons,
        }

        [EnumHandle]
        public enum State
        {
            Stop,
            WeaponsActive,
        }

        [EnumHandle]
        public enum Stats
        {
            Speed,
        }

        [Serializable]
        public class UnitDef : IDef<Unit>, Map.IPlacement
        {
            [SerializeField] int2 m_Size = new int2(1, 1);
            [SerializeField] float3 m_Pivot = new float3(0, 0f, 0f);
            [SerializeField, SelectType(typeof(Map.Layers.ILayer))]
            string m_Layer;
            #region Map.IPlacement
            int2 Map.IPlacement.Size => m_Size;
            float3 Map.IPlacement.Pivot => m_Pivot;
            TypeIndex Map.IPlacement.Layer => TypeManager.GetTypeIndex(Type.GetType(m_Layer));
            #endregion
            public List<ChildConfig> Parts = new List<ChildConfig>();

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Speed;

            [SerializeReference, ReferenceSelect(typeof(IStatValue))]
            public IStatValue Health;
        }
    }
}