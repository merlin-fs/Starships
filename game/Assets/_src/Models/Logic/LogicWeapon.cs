using System;

namespace Game.Model.Logics
{
    using Game.Model.Stats;
    using Weapons;
    using static Game.Model.Logics.Logic;
    public class LogicWeapon: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeAction(Global.Action.Init);
            logic.AddTriggerState<Weapon.WeaponInRange>(GoalHandle.FromEnum(Target.State.Found, true));
            
            logic.AddAction(Target.Action.Find)
                .AddPreconditions(Target.State.Found, false)
                .AddEffect(Target.State.Found, true)
                .Cost(1);

            logic.AddAction(Weapon.Action.Shoot)
                .AddPreconditions(Weapon.State.Shooting, true)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);
            
            logic.AddAction(Weapon.Action.Attack)
                .AddPreconditions(Weapon.State.Active, true)
                .AddPreconditions(Target.State.Found, true)
                .AddPreconditions(Weapon.State.HasAmo, true)
                .AddEffect(Weapon.State.Shooting, true)
                .Cost(1);

            logic.AddAction(Weapon.Action.Reload)
                .AddPreconditions(Weapon.State.HasAmo, false)
                .AddEffect(Weapon.State.HasAmo, true)
                .Cost(2);

            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}