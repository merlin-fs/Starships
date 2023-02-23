using System;
using Unity.Entities;
using UnityEngine;
using Common.Core;

namespace Game.Core.Prefabs
{
#if UNITY_EDITOR

    public class PrefabEnvironmentAuthoring : MonoBehaviour
    {
        [NonSerialized]
        public ObjectID ConfigID;

        class _baker : Baker<PrefabEnvironmentAuthoring>
        {
            public unsafe override void Bake(PrefabEnvironmentAuthoring authoring)
            {
                var entity = GetEntity();
                AddComponent(new BakedPrefabEnvironmentData { ConfigID = authoring.ConfigID });
            }
        }
    }
#endif
}
