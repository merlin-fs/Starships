using System;
using Unity.Entities;

namespace Game.Views
{
    public struct Particle : IComponentData
    {
        public int ID;

        public static implicit operator Particle(string value)
        {
            return new Particle { ID = value.GetHashCode(), };
        }
    }

    public struct ParticleView : IComponentData
    {
        public int ID;

        public static implicit operator ParticleView(string value)
        {
            return new ParticleView { ID = value.GetHashCode(), };
        }
    }
}