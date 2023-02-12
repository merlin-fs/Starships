using System;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public interface IStateData { }

        public struct WorldState: IBufferElementData
        {
            public bool Value;
        }

        public struct WaitState : IBufferElementData
        {
            public int Value;
        }
    }
}