using System;

using Game.Core;

using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    using static Logics.Logic;

    public partial struct Target : IComponentData, IStateData
    {
        public Entity Value;
        public LocalTransform Transform;
        public float Radius;
        public uint SoughtTeams;

        [EnumHandle]
        public enum Action
        {
            Find,
        }

        [EnumHandle]
        public enum State
        {
            Found,
            Dead,
        }
    }
}
