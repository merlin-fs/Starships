//#if UNITY_EDITOR
using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Common.Core;
using Common.Defs;

using Game.Model;
using Game.Model.Worlds;

using UnityEngine.AI;

namespace Game.Core.Prefabs
{
    public class PrefabEnvironmentAuthoring : MonoBehaviour, IConfig
    {
        [SerializeField] int2 m_Size;
        [SerializeField] float3 m_Pivot = new float3(1.5f, 0.0625f, 1.5f);

        [SerializeField, SelectType(typeof(Map.Layers.ILayer))]
        string m_Layer;

        [NonSerialized] public ObjectID ConfigID;

        [NonSerialized] public string[] Labels;

        private Entity m_Prefab;
        
        public ObjectID ID => name;
        public Entity EntityPrefab => m_Prefab;

        public void Configure(Entity root, IDefinableContext context)
        {
            m_Prefab = root;
        }

        /*
        class Baker : Baker<PrefabEnvironmentAuthoring>
        {
            public override void Bake(PrefabEnvironmentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PrefabInfo.BakedTag>(entity);
                AddComponent(entity, new PrefabInfo() {ConfigID = authoring.ConfigID, Entity = entity,});

                var fs = new FixedString128Bytes();
                fs.Append(TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(Type.GetType(authoring.m_Layer)))
                    .DebugTypeName);
                AddComponent(entity, new PrefabInfo.BakedEnvironment {Size = authoring.m_Size, Pivot = -authoring.m_Pivot / 2, Layer = fs,});

                if (authoring.TryGetComponent<BoxCollider>(out var collider))
                {
                    //transform = float4x4.TRS(ltw.Position, ltw.Rotation, new(1f, 1f, 1f)),
                    //var center = collider.transform.TransformPoint(collider.center);
                    var scale = collider.transform.lossyScale;
                    var data = new Map.NavMeshSourceData {
                        Shape = NavMeshBuildSourceShape.Box,
                        Transform = float4x4.TRS(collider.center, collider.transform.rotation, Vector3.one),
                        Size = new Vector3(collider.size.x * Mathf.Abs(scale.x), collider.size.y * Mathf.Abs(scale.y),
                            collider.size.z * Mathf.Abs(scale.z)),
                    };
                    AddComponent(entity, data);
                }

                AddBuffer<PrefabInfo.BakedLabel>(entity);
                foreach (var iter in authoring.Labels)
                {
                    var lb = new PrefabInfo.BakedLabel();
                    FixedStringMethods.CopyFromTruncated(ref lb.Label, iter);
                    AppendToBuffer(entity, lb);
                }

                var compositeScale = float4x4.Scale(authoring.transform.localScale);
                AddComponent(entity, new PostTransformMatrix {Value = compositeScale});


                var parent = authoring.transform;
                while (parent.transform.parent != null)
                    parent = parent.transform.parent;
                AddComponent(entity, new Root {Value = GetEntity(parent, TransformUsageFlags.Dynamic)});
            }
        }
        */

        private void OnDrawGizmos()
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;
            var mesh = meshFilter.sharedMesh;

            var offset = (mesh.bounds.center / 2) + (mesh.bounds.min / 2); //Vector3.zero;//
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
