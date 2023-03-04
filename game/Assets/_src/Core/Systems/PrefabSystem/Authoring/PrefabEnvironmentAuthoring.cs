using System;
using Unity.Entities;
using UnityEngine;
using Common.Core;
using Unity.Mathematics;

namespace Game.Core.Prefabs
{
    using Model.Worlds.Bulds;

#if UNITY_EDITOR

    public class PrefabEnvironmentAuthoring : MonoBehaviour
    {
        [SerializeField]
        int2 m_Size;

        [NonSerialized]
        public ObjectID ConfigID;

        class _baker : Baker<PrefabEnvironmentAuthoring>
        {
            public unsafe override void Bake(PrefabEnvironmentAuthoring authoring)
            {
                var entity = GetEntity();
                AddComponent(new BakedPrefabEnvironmentData { ConfigID = authoring.ConfigID });
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
#endif
}
