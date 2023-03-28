using System;
using Unity.Entities;
using UnityEngine;
using Common.Core;
using Common.Defs;

namespace Game.Core.Prefabs
{
    using Defs;

    using Unity.Collections;

    public struct BakedPrefabData : IBufferElementData, IEnableableComponent
    {
        public Entity Prefab;
        public ObjectID ConfigID;
    }

    public struct BakedPrefabEnvironmentData : IComponentData
    {
        public FixedString64Bytes Repository;
        public ObjectID ConfigID;
    }

    public struct PrefabStore : IComponentData, IEnableableComponent { }

#if UNITY_EDITOR

    public class PrefabsStoreConfig : MonoBehaviour
    {
        public GameObjectConfig Player;
        public GameObjectConfig Enenmy;
        //public GameObjectConfig Weapon;

        public class _baker : Baker<PrefabsStoreConfig>
        {
            public unsafe override void Bake(PrefabsStoreConfig authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PrefabStore>(entity);
                PrepareItem(authoring.Player);
                PrepareItem(authoring.Enenmy);
                //PrepareItem(authoring.Weapon);

                BakeItem(authoring.Player);
                BakeItem(authoring.Enenmy);
                //BakeItem(authoring.Weapon);
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
                GetEntity(config.PrefabObject, TransformUsageFlags.Dynamic);
                GetOrAddComponent<PrefabAuthoring>(config.PrefabObject).ConfigIDs.Add(config.ID);
                HierarchyConfig(config);
            }

            private void HierarchyConfig(GameObjectConfig config)
            {
                if (config is IConfigContainer container)
                {
                    foreach (var iter in container.Childs)
                    {
                        if (!iter.Enabled) continue;

                        if (iter.PrefabObject)
                        {
                            GetOrAddComponent<PrefabAuthoring>(iter.PrefabObject).ConfigIDs.Add(iter.Child.ID);
                        }
                        if (iter.Child is GameObjectConfig objectConfig)
                            HierarchyConfig(objectConfig);
                    }
                }
            }
        }
    }
#endif
}
