#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using Unity.Entities;
using Game.Core.Prefabs;
using Common.Core;
using System.Linq;

public class PrefabsSubScene : MonoBehaviour
{
    [SerializeField]
    AddressableAssetGroup[] m_PrefabsGroup;

    public class _baker : Baker<PrefabsSubScene>
    {
        public T GetOrAddComponent<T>(GameObject uo) where T : Component
        {
            return uo.GetComponent<T>() ?? uo.AddComponent<T>();
        }

        public unsafe override void Bake(PrefabsSubScene authoring)
        {
            foreach (var iter in authoring.m_PrefabsGroup.SelectMany(p => p.entries))
            {
                var prefab = (GameObject)iter.MainAsset;
                GetEntity(prefab, TransformUsageFlags.Dynamic);
                var component = GetOrAddComponent<PrefabEnvironmentAuthoring>(prefab);
                component.ConfigID = ObjectID.Create(prefab.name);
                component.Labels = iter.labels.ToArray();
            }
        }
    }
}
#endif