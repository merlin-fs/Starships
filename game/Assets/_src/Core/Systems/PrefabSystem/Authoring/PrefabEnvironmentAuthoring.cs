#if UNITY_EDITOR
using System;
using Unity.Entities;
using UnityEngine;
using Common.Core;
using Unity.Mathematics;

using Buildings.Environments;
using static UnityEngine.Rendering.DebugUI;
using Unity.Collections;

namespace Game.Core.Prefabs
{

    public class PrefabEnvironmentAuthoring : MonoBehaviour
    {
        [SerializeField]
        int2 m_Size;

        [NonSerialized]
        public ObjectID ConfigID;
        [NonSerialized]
        public string Repository;

        class _baker : Baker<PrefabEnvironmentAuthoring>
        {
            public unsafe override void Bake(PrefabEnvironmentAuthoring authoring)
            {
                var entity = GetEntity();
                var data = new BakedPrefabEnvironmentData
                {
                    ConfigID = authoring.ConfigID,
                };
                FixedStringMethods.CopyFromTruncated(ref data.Repository, authoring.Repository);
                
                AddComponent(data);
                AddComponent(new BuildingSize { Size = authoring.m_Size });
            }
        }

        private void OnDrawGizmos()
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;
            var mesh = meshFilter.sharedMesh;

            var offset = mesh.bounds.center / 2 + mesh.bounds.min / 2;//Vector3.zero;//
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
#endif
