using System;
using Unity.Entities;
using Unity.Mathematics;

using Game.Core;
using Game.Core.Storages;

using static Game.Model.Logics.Logic;

namespace Game.Model
{
    public interface IMoveArguments
    {
        float3 Position { get; }
        quaternion Rotation { get; }
        float Speed { get; }
    }

    [Serializable, Storage]
    public partial struct Move : IComponentData, IStateData
    {
        public float3 Position;
        public quaternion Rotation;
        public float Speed;
        public float Travel;

        [EnumHandle]
        public enum State
        {
            Init,
            MoveDone,
            PathFound,
        }
    }
}
