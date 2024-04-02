using System;
using Game.Model.Logics;
using Game.Model.Weapons;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public struct WeaponsActivate : Logic.IAction<Context>
        {
            public void Execute(ref Context context)
            {
                if (!context.Children.HasBuffer(context.Logic.Self)) return;
                
                var children = context.Children[context.Logic.Self];
                foreach (var iter in children)
                {
                    var child = context.LogicLookup[iter.Value];
                    child.SetWorldState(Weapon.State.Active, true);
                }
            }
        }
    }
}