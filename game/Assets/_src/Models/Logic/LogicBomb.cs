using System;

namespace Game.Model.Logics
{
    using Game.Model.Stats;

    using Weapons;
    using static Game.Model.Logics.Logic;
    
    public class LogicBomb: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeState(Weapon.State.NoAmmo, true);
            logic.SetInitializeState(Weapon.State.HasAmo, true);
            logic.SetInitializeState(Weapon.State.Active, false);

            logic.AddAction(Weapon.Action.Reload)
                .AddPreconditions(Weapon.State.HasAmo, true)
                .AddEffect(Weapon.State.NoAmmo, false)
                .Cost(1);

            logic.AddAction(Weapon.Action.Shooting)
                .AddPreconditions(Weapon.State.Active, true)
                .AddPreconditions(Weapon.State.NoAmmo, false)
                .AddEffect(Target.State.Dead, true)
                .Cost(4);

            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoal(Weapon.State.NoAmmo, false);
            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}