using System;

namespace Game.Model.Logics
{
    using Game.Model.Stats;

    using Units;

    using static Game.Model.Logics.Logic;
    public class LogicUnit: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeAction(Global.Action.Init);
            
            logic.SetInitializeState(Move.State.Init, false);
            logic.SetInitializeState(Unit.State.WeaponInRange, false);
            
            logic.AddTriggerState<Unit.WeaponsActivate>(GoalHandle.FromEnum(Unit.State.WeaponInRange, true));
            

            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(0);

            logic.AddAction(Unit.Action.Attack)
                .AddPreconditions(Unit.State.WeaponInRange, false)
                .AddEffect(Unit.State.WeaponInRange, true)
                .Cost(0);

            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoal(Move.State.Init, true);
            logic.EnqueueGoal(Unit.State.WeaponInRange, true);
        }
    }
}