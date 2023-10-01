using System;
using System.Collections.Generic;
using Game.Model.Logics;
using Game.Model.Worlds;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public struct FindPathRadius : Logic.IAction<Context>
        {
            public void Execute(ref Context context)
            {
                var pos = context.LookupMapTransform[context.Aspect.Target.Value].Position;
                pos = Map.GetCells(pos, 1, null).RandomElement();
                context.Writer.AddComponent(context.SortKey, context.Aspect.Self, new Move.Target {Value = pos});
                var speed = context.Aspect.Stat(Unit.Stats.Speed).Value;
                context.Writer.AddComponent(context.SortKey, context.Aspect.Self, new Move {Speed = speed});
            }
        }
    }
}