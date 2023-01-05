using System;
using Unity.Entities;
using UnityEngine;

namespace Game.Core.Prefabs
{
    using Defs;

    public struct PrefabData : IBufferElementData, IEnableableComponent
    {
        public Entity Prefab;
    }

    public struct PrefabStore : IComponentData, IEnableableComponent { }

    public class PrefabsStoreConfig : MonoBehaviour
    {
        public GameObjectConfig Player;
        public GameObjectConfig Enenmy;
        public GameObjectConfig Weapon;

        public class _baker : Baker<PrefabsStoreConfig>
        {
            public unsafe override void Bake(PrefabsStoreConfig authoring)
            {
#if UNITY_EDITOR
                var buffer = AddBuffer<PrefabData>();
                buffer.Add(new PrefabData { Prefab = GetEntity(authoring.Player.PrefabObject) });
                buffer.Add(new PrefabData { Prefab = GetEntity(authoring.Enenmy.PrefabObject) });
                buffer.Add(new PrefabData { Prefab = GetEntity(authoring.Weapon.PrefabObject) });
#endif
            }
        }
    }
}
