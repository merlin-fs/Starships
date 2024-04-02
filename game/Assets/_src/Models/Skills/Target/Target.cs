using System;
using Unity.Entities;
using Game.Core;
using Game.Model.Logics;

namespace Game.Model
{
    public partial struct Target : IComponentData, Logic.IStateData
    {
        public Entity Value;

        public struct Query: IComponentData
        {
            public float Radius;
            public uint SearchTeams;
        }

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
