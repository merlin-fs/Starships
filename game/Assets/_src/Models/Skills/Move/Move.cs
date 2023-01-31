using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model
{
    [Serializable]

    public struct Move : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
        public float Speed;

        public enum State
        {
            Init,
            MoveTo,
        }

        public enum Condition
        {
            Init,
            MoveDone,
        }
    }
}
