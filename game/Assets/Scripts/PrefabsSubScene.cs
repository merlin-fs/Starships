#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using Unity.Entities;
using Game.Core.Prefabs;
using Common.Core;

public class PrefabsSubScene : MonoBehaviour
{
    [SerializeField]
    AddressableAssetGroup m_Prefabs;

    public class _baker : Baker<PrefabsSubScene>
    {
        public T GetOrAddComponent<T>(GameObject uo) where T : Component
        {
            return uo.GetComponent<T>() ?? uo.AddComponent<T>();
        }

        public unsafe override void Bake(PrefabsSubScene authoring)
        {
            foreach(var iter in authoring.m_Prefabs.entries)
            {
                var prefab = (GameObject)iter.MainAsset;
                GetEntity(prefab);
                GetOrAddComponent<PrefabEnvironmentAuthoring>(prefab).ConfigID = ObjectID.Create(prefab.name);
            }
        }
    }
}
#endif