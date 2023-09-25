using System;
using Game.Model.Stats;
using Game.Model.Units;

namespace Game.Model.Logics
{
    using static Game.Model.Logics.Logic;

    public class LogicUnit: ILogic
    {
        public void Initialize(LogicDef logic)
        {
            logic.Initialize(Global.Action.Init);
            
            logic.SetState(Move.State.Init, true);
            logic.SetState(Unit.State.WeaponInRange, false);

            logic.CustomAction<Unit, Unit.WeaponsActivate>()
                .BeforeAction(Unit.Action.WeaponsActivate);

            //logic.AfterAction<Weapon, Weapon.WeaponInRange, Unit.State.WeaponInRange>(true);
            //logic.AddTriggerState<Unit.WeaponsActivate>(GoalHandle.FromEnum(Unit.State.WeaponInRange, true));
            

            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(0);

            logic.AddAction(Unit.Action.WeaponsActivate)
                .AddPreconditions(Unit.State.WeaponInRange, false)
                .AddEffect(Unit.State.WeaponInRange, true)
                .Cost(0);

            logic.AddAction(Unit.Action.Attack)
                .AddPreconditions(Unit.State.WeaponInRange, true)
                .AddPreconditions(Target.State.Found, true)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);
            
            
            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoal(Move.State.Init, true);
            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}