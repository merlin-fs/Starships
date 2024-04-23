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

        public struct WorldChanged : IBufferElementData
        {
            public GoalHandle Value;
        }

        public struct Plan : IBufferElementData
        {
            public LogicActionHandle Value;
            public static implicit operator Plan(LogicActionHandle value) => new Plan { Value = value }; 
        }
    }
}