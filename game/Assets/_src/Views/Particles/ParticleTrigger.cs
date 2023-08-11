using System;

using Game.Core;

using Unity.Entities;

namespace Game.Views
{
    public struct ParticleTrigger : IBufferElementData
    {
        public EnumHandle Action;
        public Hash128 VfxID;
        public Entity Target;

        public bool Scale;
        public bool Position;
        public bool Rotation;
        public float ScaleTime;
    }
}