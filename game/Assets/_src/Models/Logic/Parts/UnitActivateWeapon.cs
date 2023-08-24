using System;
using Unity.Entities;
using Game.Model.Units;
using Game.Model.Weapons;

namespace Game.Model.Logics
{
    public struct UnitActivateWeapon : Logic.ISlice
    {
        public bool IsConditionHit(ref Logic.SliceContext context)
        {
            return context.Logic.IsCurrentAction(Unit.Action.ActiveWeapons);
        }

        public void Execute(ref Logic.SliceContext context)
        {
            if (!context.Children.HasBuffer(context.Logic.Self)) return;
            var children = context.Children[context.Logic.Self];
            foreach (var iter in children)
            {
                var child = context.Lookup[iter.Value];
                child.SetWorldState(Weapon.State.Active, true);
            }
            context.Logic.SetWorldState(Unit.State.WeaponsActive, true);
        }
    }
}