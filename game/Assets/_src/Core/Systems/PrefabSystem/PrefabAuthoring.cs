using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Common.Core;
using Common.Defs;
using Unity.Transforms;
using Game.Model;

namespace Game.Core.Prefabs
{
#if UNITY_EDITOR
    public class PrefabAuthoring : MonoBehaviour
    {
        [SerializeField, SelectType(typeof(IDefineable))]
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

                if (parent != authoring.transform)
                {
                    AddComponent<Part>(new Part { Unit = GetEntity(parent) });
                }

            }
        }
    }
#endif
}
