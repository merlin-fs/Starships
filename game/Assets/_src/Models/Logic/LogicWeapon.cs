using System;

namespace Game.Model.Logics
{
    using Weapons;
    using static Game.Model.Logics.Logic;
    public class LogicWeapon: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeState(Weapon.State.NoAmmo, true);
            logic.SetInitializeState(Weapon.State.HasAmo, true);

            logic.AddAction(Target.Action.Find)
                .AddPreconditions(Target.State.Found, false)
                .AddPreconditions(Weapon.State.NoAmmo, false)
                .AddEffect(Target.State.Found, true)
                .Cost(1);

            logic.AddAction(Weapon.Action.Shooting)
                .AddPreconditions(Target.State.Found, true)
                .AddPreconditions(Weapon.State.NoAmmo, false)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);

            /*
            Logic.AddAction(Weapon.Action.bomb)
                .AddEffect(Target.State.Dead, true)
                .Cost(8);
            */

            logic.AddAction(Weapon.Action.Reload)
                .AddPreconditions(Weapon.State.NoAmmo, true)
                //.AddPreconditions(Weapon.State.HasAmo, true)
                .AddEffect(Weapon.State.NoAmmo, false)
                .Cost(2);

            logic.EnqueueGoal(Weapon.State.NoAmmo, false);
            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}