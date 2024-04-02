using Unity.Entities;
using Unity.Mathematics;

namespace Game.Core
{
    internal partial struct Skin
    {
        struct Root : IComponentData
        {
            public Entity Value;
        }

        struct Bone : IBufferElementData
        {
            public float4x4 BindPose;
            public Entity Value;
        }
    }
}