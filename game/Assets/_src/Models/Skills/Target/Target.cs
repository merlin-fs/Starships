using System;

using Game.Core;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Model
{
    using static Logics.Logic;

    public partial struct Target : IComponentData, IStateData
    {
        public Entity Value;
        public LocalToWorld Transform;
        public float Radius;
        public uint SearchTeams;

        [EnumHandle]
        public enum Action
        {
            Find,
        }

        public enum Params
        {
            Radius,
            SearchTeams,
        }

        [EnumHandle]
        public enum State
        {
            Found,
            Dead,
        }
    }
}
