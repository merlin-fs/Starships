using System;
using Unity.Entities;
using UnityEngine;
using Common.Core;
using Common.Defs;

namespace Game.Core.Prefabs
{
    using Defs;
    using Unity.Entities.Hybrid.Baking;

    public struct BakedPrefabData : IBufferElementData, IEnableableComponent
    {
        public Entity Prefab;
        public ObjectID ConfigID;
    }

    public struct PrefabStore : IComponentData, IEnableableComponent { }

#if UNITY_EDITOR

    public class PrefabsStoreConfig : MonoBehaviour
    {
        public GameObjectConfig Player;
        public GameObjectConfig Enenmy;
        public GameObjectConfig Weapon;

        public class _baker : Baker<PrefabsStoreConfig>
        {
            public unsafe override void Bake(PrefabsStoreConfig authoring)
            {
                AddComponent<PrefabStore>();
                PrepareItem(authoring.Player);
                PrepareItem(authoring.Enenmy);
                PrepareItem(authoring.Weapon);

                BakeItem(authoring.Player);
                BakeItem(authoring.Enenmy);
                BakeItem(authoring.Weapon);
            }

            public T GetOrAddComponent<T>(GameObject uo) where T : Component
            {
                return uo.GetComponent<T>() ?? uo.AddComponent<T>();
            }

            private void PrepareItem(GameObjectConfig config)
            {
                var list = config.PrefabObject.GetComponentsInChildren<PrefabAuthoring>();
                foreach (var iter in list)
                    iter.ConfigIDs.Clear();
            }

            private void BakeItem(GameObjectConfig config)
            {
                GetEntity(config.PrefabObject);
                GetOrAddComponent<PrefabAuthoring>(config.PrefabObject).ConfigIDs.Add(config.ID);

                if (config is IConfigContainer container)
                {
                    foreach (var iter in container.Childs)
                    {
                        if (iter.PrefabObject)
                        {
                            GetOrAddComponent<PrefabAuthoring>(iter.PrefabObject).ConfigIDs.Add(iter.Child.ID);
                        }
                    }
                }
            }
        }
    }
#endif
}
