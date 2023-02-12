using System;

namespace Game.Model.Logics
{
    using Weapons;
    using static Game.Model.Logics.Logic;
    
    public class LogicBomb: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeState(Weapon.State.NoAmmo, true);
            logic.SetInitializeState(Weapon.State.HasAmo, true);

            logic.AddAction(Weapon.Action.Reload)
                .AddPreconditions(Weapon.State.HasAmo, true)
                .AddEffect(Weapon.State.NoAmmo, false)
                .Cost(1);

            logic.AddAction(Weapon.Action.Shoot)
                .AddPreconditions(Move.State.MoveDone, true)
                .AddPreconditions(Weapon.State.NoAmmo, false)

                .AddEffect(Weapon.State.HasAmo, false)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);

            logic.AddGoal(Target.State.Dead, true);
        }
    }
}