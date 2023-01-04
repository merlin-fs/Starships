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
        public ScriptableConfig Player;
        public ScriptableConfig Enenmy;
        public ScriptableConfig Weapon;

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
