using System;
using Game.Model.Stats;
using Game.Model.Units;
using static Game.Model.Logics.Logic;

namespace Game.Model.Logics
{
    public class LogicUnit: ILogic
    {
        public void Initialize(LogicDef logic)
        {
            //logic.Initialize(Global.Action.Init);

            logic.SetWorldState(Move.State.Init, true);
            logic.SetWorldState(Unit.State.WeaponInRange, false);

            /*
            logic.Action<Unit.WeaponsActivate>()
                .AddPreconditions(Unit.State.WeaponInRange, false)
                .AddAfter();
            logic.Action<Unit.WeaponsActivate>()
                .AddPreconditions(Move.State.Init, false)
                .AddAfter();

            //logic.AfterAction<Weapon, Weapon.WeaponInRange, Unit.State.WeaponInRange>(true);
            //logic.AddTriggerState<Unit.WeaponsActivate>(GoalHandle.FromEnum(Unit.State.WeaponInRange, true));
            
            /*
            logic.AddTransition(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(0);
            */

            //logic.AddTransition<Logic.InitTag>()
                
            logic.AddTransition<Unit, Unit.WeaponsActivate>()
                .AddPreconditions(Unit.State.WeaponInRange, false)
                .AddEffect(Unit.State.WeaponInRange, true)
                .Cost(0);

            logic.AddTransition<Unit, Unit.Attack>()
                .AddPreconditions(Unit.State.WeaponInRange, true)
                .AddPreconditions(Target.State.Found, true)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);
            
            
            logic.AddTransition<Unit, Unit.Destroy>()
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoal(Move.State.Init, true);
            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}