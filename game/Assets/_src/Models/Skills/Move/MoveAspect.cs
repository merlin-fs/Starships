using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model
{
    public partial struct Move
    {
        public readonly partial struct Aspect: IAspect
        {
            private readonly Entity m_Self;
            private readonly RefRW<Move> m_Move;
            public bool SetTarget(float3 value, float speed)
            {
                return true;
            }
        }
    }
}