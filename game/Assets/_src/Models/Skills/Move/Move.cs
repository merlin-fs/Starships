using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model
{
    [Serializable]
    public struct Move : IComponentData
    {
        public float3 Position;

        public enum State
        {
            Init,
        }

        public enum Result
        {
            Done,
        }
    }
}
