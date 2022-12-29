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
                var buffer = AddBuffer<PrefabData>();
                //this.SetComponentEnabled<PrefabData>(GetEntity(), false);
                
                buffer.Add(new PrefabData { Prefab = GetEntity(authoring.Config2.Prefab) });
                buffer.Add(new PrefabData { Prefab = GetEntity(authoring.Config1.Prefab) });
            }
        }
    }
}
