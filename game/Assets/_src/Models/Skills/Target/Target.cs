using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model
{
    [Serializable]
    public struct Target : IComponentData
    {
        public Entity Value;
        //public Team Team;

        public enum State
        {
            Find,
        }
    }
}
