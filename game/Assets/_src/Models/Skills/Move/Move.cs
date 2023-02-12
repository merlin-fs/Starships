using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model
{
    using static Game.Model.Logics.Logic;

    public interface IMoveArguments
    {
        float3 Position { get; }
        quaternion Rotation { get; }
        float Speed { get; }
    }

    [Serializable]
    public partial struct Move : IComponentData, IStateData
    {
        public float3 Position;
        public quaternion Rotation;
        public float Speed;

        public enum Action
        {
            Init,
            MoveTo,
        }

        public enum State
        {
            Init,
            MoveDone,
        }
    }
}
