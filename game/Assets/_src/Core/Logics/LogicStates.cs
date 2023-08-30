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

        public struct InitTag : IComponentData{}
        
        public struct WorldState : IBufferElementData
        {
            public bool Value;
        }

        public struct Plan : IBufferElementData
        {
            public EnumHandle Value;
            public static implicit operator Plan(EnumHandle value) => new Plan { Value = value }; 
        }
    }
}