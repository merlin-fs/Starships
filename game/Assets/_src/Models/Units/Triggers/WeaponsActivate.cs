using System;
using Game.Model.Logics;
using Game.Model.Weapons;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public struct WeaponsActivate : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                if (!context.Children.HasBuffer(context.Entity)) return;
                
                var children = context.Children[context.Entity];
                foreach (var iter in children)
                {
                    context.SetWorldState(iter.Value, Weapon.State.Active, true);
                }
            }
        }
    }
}