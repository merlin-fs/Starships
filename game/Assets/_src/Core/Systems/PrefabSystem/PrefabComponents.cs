using System;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Common.Core;

using Game.Core.Storages;

using Reflex.Core;


namespace Game.Core.Prefabs
{
    [Serializable, Storage]
    public partial struct PrefabInfo: IComponentData
    {
        private Entity m_Entity;
        public Entity Entity { get => m_Entity; set => m_Entity = value; }

        private ObjectID m_ConfigID;
        public ObjectID ConfigID { get => m_ConfigID; set => m_ConfigID = value; }

        public struct BakedTag : IComponentData {}

        public class ContextReference : IComponentData
        {
            public Container Value;
        }

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