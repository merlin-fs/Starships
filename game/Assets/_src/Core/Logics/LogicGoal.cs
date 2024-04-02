using System;

using Game.Core;

using Unity.Entities;
using Unity.Properties;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public struct Goal : IBufferElementData
        {
            [CreateProperty] private string ID => State.ToString();
            public EnumHandle State;
            public bool Value;
            public bool Repeat;
        }
    }
}