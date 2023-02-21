using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    using static Game.Model.Logics.Logic;

    [Serializable]
    public partial struct Target : IComponentData, IStateData
    {
        public Entity Value;
        public WorldTransform WorldTransform;
        public float Radius;
        public uint SoughtTeams;

        public enum Action
        {
            Find,
        }

        public enum State
        {
            Found,
            Dead,
        }
    }
}
