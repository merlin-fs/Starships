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

        public enum State
        {
            Init,
            MoveTo,
        }

        public enum Result
        {
            Done,
        }
    }
}
