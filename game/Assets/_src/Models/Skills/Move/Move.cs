using System;

using Game.Core;
using Game.Core.Saves;

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

    [Serializable, Saved]
    public partial struct Move : IComponentData, IStateData
    {
        public float3 Position;
        public quaternion Rotation;
        public float Speed;

        [EnumHandle]
        public enum Action
        {
            Init,
            MoveToTarget,
            MoveToPosition,
        }

        [EnumHandle]
        public enum State
        {
            Init,
            MoveDone,
        }
    }
}
