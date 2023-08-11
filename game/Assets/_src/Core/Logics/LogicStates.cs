using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Ext;

using Game.Core;

using Unity.Entities;

namespace Game.Model.Logics
{
    public partial struct Logic
    {
        public interface IStateData { }

        public struct WorldState : IBufferElementData
        {
            public bool Value;
        }
    }
}