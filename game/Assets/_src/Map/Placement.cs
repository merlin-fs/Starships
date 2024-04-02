using System;
using System.Runtime.InteropServices;

using Common.Defs;

using Game.Core.Saves;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public interface IPlacement
        {
            int2 Size { get; }
            float3 Pivot { get; }
            TypeIndex Layer  { get; }
        }
        
        [Serializable, Saved]
        public struct Transform : IComponentData
        {
            public int2 Position;
            public float2 Rorate;
        }
        
        public struct Target: IComponentData
        {
            public int2 Value;
        }
        
        public readonly struct Placement: IComponentData
        {
            private readonly RefLink<IPlacement> m_RefLink;
            public IPlacement Value => m_RefLink.Value;

            public Placement(RefLink<IPlacement> link)
            {
                m_RefLink = link;
            }
        }
    }
}