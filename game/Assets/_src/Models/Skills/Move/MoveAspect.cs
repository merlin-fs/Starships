using System;
using Game.Model.Worlds;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    public partial struct Move
    {
        public readonly partial struct Aspect: IAspect
        {
            private readonly Entity m_Self;
            private readonly RefRW<Target> m_Target;
            private readonly RefRW<Map.Transform> m_MapTransform;
            private readonly RefRW<LocalTransform> m_LocalTransform;
            public ref readonly LocalTransform LocalTransformRO => ref m_LocalTransform.ValueRO;
            public ref LocalTransform LocalTransformRW => ref m_LocalTransform.ValueRW;
        }
    }
}