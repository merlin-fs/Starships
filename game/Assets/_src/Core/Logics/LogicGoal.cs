using System;
using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public struct Goal : IBufferElementData
        {
            public LogicHandle State;
            public bool Value;
            public bool Repeat;
        }
    }
}