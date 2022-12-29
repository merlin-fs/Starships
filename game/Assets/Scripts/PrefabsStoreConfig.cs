using System;
using Common.Defs;
using Game.Model.Units;
using Unity.Entities;
using UnityEngine;

namespace Game.Core.Prefabs
{
    public struct PrefabData : IBufferElementData, IEnableableComponent
    {
        public Entity Prefab;
    }

    public struct PrefabStore : IComponentData, IEnableableComponent { }

    public class PrefabsStoreConfig : MonoBehaviour
    {
        public ScriptableConfig Config1;
        public ScriptableConfig Config2;

        public class _baker : Baker<PrefabsStoreConfig>
        {
            public unsafe override void Bake(PrefabsStoreConfig authoring)
            {
#if UNITY_EDITOR
                var buffer = AddBuffer<PrefabData>();
                buffer.Add(new PrefabData { Prefab = GetEntity(authoring.Config2.PrefabObject) });
                buffer.Add(new PrefabData { Prefab = GetEntity(authoring.Config1.PrefabObject) });
#endif
            }
        }
    }
}
