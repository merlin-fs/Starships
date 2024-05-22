//#if UNITY_EDITOR
using System;
using System.Threading.Tasks;

using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Common.Core;
using Common.Defs;

using Game.Model.Worlds;

using UnityEngine.AI;

namespace Game.Core.Prefabs
{
    public class PrefabEnvironmentAuthoring : MonoBehaviour, Map.IPlacement, IViewPrefab
    {
        [SerializeField] int2 m_Size;
        [SerializeField] float3 m_Pivot = new float3(1.5f, 0.0625f, 1.5f);

        [SerializeField, SelectType(typeof(Map.Layers.ILayer))]
        string m_Layer;

        [NonSerialized] public ObjectID ConfigID;

        public ObjectID ID => name;

        public void Configure(Entity entity, IDefinableContext context)
        {
            context.AddComponentData(entity, new PrefabInfo() {ConfigID = ConfigID});
            context.AddComponentData(entity, new LocalTransform());
            context.AddComponentData(entity, new Map.Move());
            
            
            var fs = new FixedString128Bytes();
            
            fs.Append(TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(Type.GetType(m_Layer))).DebugTypeName);
            context.AddComponentData(entity, new PrefabInfo.BakedEnvironment {Size = m_Size, Pivot = -m_Pivot / 2, Layer = fs,});
            if (TryGetComponent<BoxCollider>(out var collider))
            {
                var scale = collider.transform.lossyScale;
                var data = new Map.NavMeshSourceData {
                    Shape = NavMeshBuildSourceShape.Box,
                    Transform = float4x4.TRS(collider.center, collider.transform.rotation, Vector3.one),
                    Size = new Vector3(collider.size.x * Mathf.Abs(scale.x), collider.size.y * Mathf.Abs(scale.y),
                        collider.size.z * Mathf.Abs(scale.z)),
                };
                context.AddComponentData(entity, data);
            }

            var compositeScale = float4x4.Scale(transform.localScale);
            context.AddComponentData(entity, new PostTransformMatrix {Value = compositeScale});
        }
        #region Map.IPlacement
        int2 Map.IPlacement.Size => m_Size;
        float3 Map.IPlacement.Pivot => m_Pivot;
        TypeIndex Map.IPlacement.Layer => TypeManager.GetTypeIndex(Type.GetType(m_Layer));
        #endregion
        

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

        public Task<GameObject> GetViewPrefab()
        {
            return Task.FromResult(gameObject);
        }
    }
}
