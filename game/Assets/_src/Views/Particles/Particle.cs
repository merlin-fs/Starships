using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Views
{
    public struct Particle : IBufferElementData
    {
        public Entity Target;
        public int StateID;
    }

    public static class ParticleManager
    {
        public static void Play(Entity entity, int stateID, WorldTransform transform)
        {

        }
    }

}