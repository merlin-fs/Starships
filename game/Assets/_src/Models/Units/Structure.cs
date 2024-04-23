using System;
using Common.Defs;

using Game.Model.Worlds;

using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Units
{
    [Serializable]
    public struct Structure : IUnit, IDefinable, IComponentData, IDefinableCallback
    {
        [Serializable]
        public class StructureDef : IDef<Structure>, Map.IPlacement
        {
            public int2 Size { get; set; }
            public float3 Pivot{ get; set; }
            public TypeIndex Layer{ get; set; }
        }

        private RefLink<StructureDef> RefLink { get; }
        public StructureDef Def => RefLink.Value;

        public Structure(RefLink<StructureDef> config)
        {
            RefLink = config;
        }

        public void AddComponentData(Entity entity, IDefinableContext context)
        {
            context.AddComponentData(entity, new Map.Placement(RefLink<Map.IPlacement>.Copy(RefLink)));
            context.AddComponentData(entity, new Map.Move());
        }

        public void RemoveComponentData(Entity entity, IDefinableContext context){}
    }
}