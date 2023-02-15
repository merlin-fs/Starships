using System;
using Unity.Entities;

namespace Game.Views
{
    using Model.Logics;

    public struct Particle : IBufferElementData
    {
        public LogicHandle Action;
        public Hash128 VfxID;
        public Entity Target;

        public bool Scale;
        public bool Position;
        public bool Rotation;
    }
}