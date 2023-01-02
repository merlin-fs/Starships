using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    [Serializable]
    public struct Target : IComponentData
    {
        public Entity Value;
        public uint SoughtTeams;
        public WorldTransform WorldTransform;

        public enum State
        {
            Find,
        }

        public enum Result
        {
            Found,
            NoTarget,
        }
    }
}
