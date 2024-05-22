using System;
using Game.Core;
using Game.Model.Logics;

using Unity.Entities;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public readonly struct Context: Logic.ILogicContext<Unit>
        {
            public LogicHandle LogicHandle => LogicHandle.From<Unit>();
            public Entity Entity { get; }
            public float Delta { get; }
            public void SetWorldState<T>(Entity entity, T worldState, bool value)
                where T : struct, IConvertible => this.SetWorldState(entity, GoalHandle.FromEnum(worldState, value)); 
            public void SetWorldState(Entity entity, GoalHandle value)
            {
                throw new NotImplementedException();
            }

            public BufferLookup<ChildEntity> Children { get; }
        }
    }
}