using System;
using Game.Model.Logics;
using Game.Model.Units;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct WeaponInRange : Logic.ITrigger
        {
            public void Execute(ref Logic.LogicContext context)
            {
                var unitLogic = context.Lookup[context.Logic.Root];
                unitLogic.SetWorldState(Unit.State.WeaponInRange, true);
            }
        }
    }
}