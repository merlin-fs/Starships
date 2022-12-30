using System;
using Unity.Entities;

namespace Game.Model
{
    [Serializable]
    public struct Target : IComponentData
    {
        public Entity Value;
        public uint SoughtTeams;

        public enum State
        {
            Find,
        }
    }
}
