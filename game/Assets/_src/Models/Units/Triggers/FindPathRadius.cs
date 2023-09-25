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
                pos = Map.GetCells(pos, 5, null).RandomElement();
            }
        }
    }
}