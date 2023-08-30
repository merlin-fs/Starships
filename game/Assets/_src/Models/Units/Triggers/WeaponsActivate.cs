using System;
using Game.Model.Logics;
using Game.Model.Weapons;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public struct WeaponsActivate : Logic.ITrigger
        {
            public void Execute(ref Logic.LogicContext context)
            {
                if (!context.Children.HasBuffer(context.Logic.Self)) return;
                var children = context.Children[context.Logic.Self];
                foreach (var iter in children)
                {
                    var child = context.Lookup[iter.Value];
                    child.SetWorldState(Weapon.State.Active, true);
                }
            }
        }
    }
}