using System;
using Unity.Entities;

namespace Game.Model.Logics
{
    using static Game.Model.Logics.Logic;

    public partial struct Logic
    {
        public interface IStateData { }

        public struct WorldState : IBufferElementData
        {
            public bool Value;
        }
    }

    public static class LogicWorldStateExt
    {
        public static bool GetWorldState(this DynamicBuffer<WorldState> buff, LogicDef def, Enum worldState)
        {
            var index = def.StateMapping[LogicHandle.FromEnumTest(worldState)].Index;
            return buff[index].Value;
        }
        public static bool HasWorldState(this DynamicBuffer<WorldState> buff, LogicDef def, Enum worldState, bool value)
        {
            return GetWorldState(buff, def, worldState) == value;
        }

    }
}