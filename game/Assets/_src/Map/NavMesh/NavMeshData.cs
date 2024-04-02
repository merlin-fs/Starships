using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.AI;

namespace Game.Model.Worlds
{
    public partial struct Map
    {
        public struct NavMeshBuildTag: IComponentData {}
        public struct NavMeshSourceData : IComponentData
        {
            public NavMeshBuildSourceShape Shape;
            public float4x4 Transform;
            public float3 Size;
        }
    }
}
