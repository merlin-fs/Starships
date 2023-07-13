//#if UNITY_EDITOR
using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Common.Core;
using Buildings.Environments;

using Game.Model;
using Game.Model.Worlds;

namespace Game.Core.Prefabs
{
    public class PrefabEnvironmentAuthoring : MonoBehaviour
    {
        [SerializeField]
        int2 m_Size;
        [SerializeField, SelectType(typeof(Map.Layers.ILayer))]
        string m_Layer;

        [NonSerialized]
        public ObjectID ConfigID;

        [NonSerialized]
        public string[] Labels;

        class Baker : Baker<PrefabEnvironmentAuthoring>
        {
            public unsafe override void Bake(PrefabEnvironmentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent<BakedPrefabTag>(entity);
                AddComponent(entity, new BakedPrefab()
                {
                    ConfigID = authoring.ConfigID,
                    Prefab = entity,
                });

                var fs = new FixedString128Bytes();
                fs.Append(TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(Type.GetType(authoring.m_Layer))).DebugTypeName);
                AddComponent(entity, new BakedEnvironment
                {
                    Size = authoring.m_Size,
                    Layer = fs,
                });

                AddBuffer<BakedPrefabLabel>(entity);
                foreach(var iter in authoring.Labels)
                {
                    var lb = new BakedPrefabLabel();
                    FixedStringMethods.CopyFromTruncated(ref lb.Label, iter);
                    AppendToBuffer(entity, lb);
                }
                var compositeScale = float4x4.Scale(authoring.transform.localScale);
                AddComponent(entity, new PostTransformMatrix { Value = compositeScale });


                var parent = authoring.transform;
                while (parent.transform.parent != null)
                    parent = parent.transform.parent;
                AddComponent(entity, new Root { Value = GetEntity(parent, TransformUsageFlags.Dynamic) });
            }
        }

        private void OnDrawGizmos()
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;
            var mesh = meshFilter.sharedMesh;

            var offset = (mesh.bounds.center / 2) + (mesh.bounds.min / 2);//Vector3.zero;//
            var size = new Vector3(1, 0, 1) * mesh.bounds.extents.x;

            for (int x = 0; x < m_Size.x; x++)
            {
                for (int y = 0; y < m_Size.y; y++)
                {
                    Gizmos.color = (x + y) % 2 == 0
                        ? new Color(0f, 0f, 0f, 0.5f)
                        : new Color(1f, 1f, 1f, 0.5f);

                    //transform.
                    Gizmos.DrawCube(transform.position + new Vector3(x * size.x, 0, y * size.z) + offset, size);
                }
            }
        }
    }
}
//#endif
