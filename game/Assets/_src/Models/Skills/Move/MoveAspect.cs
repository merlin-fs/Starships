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
            
            public Move Move { get => m_Move.ValueRO; set => m_Move.ValueRW = value; }
            public bool SetTarget(float3 value, float speed)
            {
                return true;
            }
        }
    }
}