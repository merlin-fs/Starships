using System;
using Game.Model.Logics;
using Game.Model.Units;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct WeaponInRange : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                //var unitLogic = context.LogicLookup[context.Weapon.Root];
                //unitLogic.SetWorldState(Unit.State.WeaponInRange, true);
            }
        }
    }
}