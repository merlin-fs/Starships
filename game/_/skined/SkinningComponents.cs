using Unity.Entities;
using Unity.Mathematics;

namespace AnimationSystem
{
    public struct An_BoneTag : IComponentData { }

    public struct An_RootTag : IComponentData { }

    public struct An_BoneEntity : IBufferElementData
    {
        public Entity Value;
    }

    public struct An_RootEntity : IComponentData
    {
        public Entity Value;
    }

    public struct An_BindPose : IBufferElementData
    {
        public float4x4 Value;
    }
    
    [TemporaryBakingType]
    internal struct An_SkinnedMeshTag : IComponentData
    {
    }
}