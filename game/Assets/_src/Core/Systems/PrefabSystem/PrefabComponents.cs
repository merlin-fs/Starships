using System;
using Unity.Entities;
using UnityEngine;
using Common.Core;
using Common.Defs;
using Hash128 = Unity.Entities.Hash128;

namespace Game.Core.Prefabs
{
    public struct PrefabTargetData : IComponentData
    {
        public bool IsChild;
        public Hash128 PrefabID;
        public TypeIndex Type;
        public Entity Target;
    }

    public struct PreparePrefabData : IBufferElementData
    {
        public ObjectID ID;
        public Entity Entity;
    }
}
