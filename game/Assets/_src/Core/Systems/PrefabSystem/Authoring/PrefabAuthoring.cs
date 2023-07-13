using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Common.Core;

namespace Game.Core.Prefabs
{
#if UNITY_EDITOR
    using Model;
    
    public class PrefabAuthoring : MonoBehaviour
    {
        [NonSerialized]
        public ObjectID ConfigID;

        class Baker : Baker<PrefabAuthoring>
        {
            public override void Bake(PrefabAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<BakedPrefabTag>(entity);
                AddComponent(entity, new BakedPrefab()
                {
                    ConfigID = authoring.ConfigID,
                    Prefab = entity,
                });
                
                var parent = authoring.transform;
                while (parent.transform.parent != null)
                    parent = parent.transform.parent;
                AddComponent(entity, new Root { Value = GetEntity(parent, TransformUsageFlags.Dynamic) });
            }
        }
    }
#endif
}
