using System;

using Game.Model.Stats;
using Game.Model.Weapons;
using static Game.Model.Logics.Logic;

namespace Game.Model.Logics
{
    public class LogicWeapon: ILogic
    {
        public void Initialize(LogicDef logic)
        {
            logic.SetWorldState(Weapon.State.Active, true);
            
            
            logic.AddTransition<Target, Target.Find>()
                .AddAction<Weapon, Weapon.FindTarget>()
                .AddPreconditions(Target.State.Found, false)
                .AddEffect(Target.State.Found, true)
                .Cost(1);

            logic.AddTransition<Weapon, Weapon.Shoot>()
                .AddPreconditions(Weapon.State.TargetLocked, true)
                .AddPreconditions(Weapon.State.Shooting, true)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);
            
            logic.AddTransition<Weapon, Weapon.Attack>()
                .AddAction<Weapon, Weapon.TrackingTarget>()
                .AddAction<Move, Move.WeaponTargetLocked>()

                .AddPreconditions(Weapon.State.Active, true)
                .AddPreconditions(Target.State.Found, true)
                .AddPreconditions(Weapon.State.HasAmo, true)
                .AddEffect(Weapon.State.TargetLocked, true)
                .AddEffect(Weapon.State.Shooting, true)
                .Cost(1);

            logic.AddTransition<Weapon, Weapon.Reload>()
                .AddPreconditions(Weapon.State.HasAmo, false)
                .AddEffect(Weapon.State.HasAmo, true)
                .Cost(2);

            logic.AddTransition<Weapon, Weapon.Destroy>()
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}