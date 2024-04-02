using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Common.Core;

using Game.Core.Saves;

namespace Game.Core.Prefabs
{
    [Serializable, Saved]
    public partial struct PrefabInfo: IComponentData
    {
        public Entity Entity;
        public ObjectID ConfigID;
        
        public struct BakedTag : IComponentData {}

        public struct BakedLabel : IBufferElementData
        {
            public FixedString64Bytes Label;
        }

        public struct BakedEnvironment : IComponentData
        {
            public int2 Size;
            public float3 Pivot;
            public FixedString128Bytes Layer;
        }
    }
}