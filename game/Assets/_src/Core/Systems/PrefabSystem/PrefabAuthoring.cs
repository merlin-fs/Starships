using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Common.Core;
using Common.Defs;

namespace Game.Core.Prefabs
{
#if UNITY_EDITOR
    using Model;

    public class PrefabAuthoring : MonoBehaviour
    {
        [SerializeField, SelectType(typeof(IDefinable))]
        string m_Type;

        [NonSerialized]
        public HashSet<ObjectID> ConfigIDs = new HashSet<ObjectID>();

        class _baker : Baker<PrefabAuthoring>
        {
            public unsafe override void Bake(PrefabAuthoring authoring)
            {
                var buffer = AddBuffer<BakedPrefabData>();
                var entity = GetEntity();
                foreach (var iter in authoring.ConfigIDs)
                {
                    buffer.Add(new BakedPrefabData
                    {
                        ConfigID = iter,
                        Prefab = entity,
                    });
                }

                var parent = authoring.transform;
                while (parent.transform.parent != null)
                    parent = parent.transform.parent;
                AddComponent<Root>(new Root { Value = GetEntity(parent) });
            }
        }
    }
#endif
}
