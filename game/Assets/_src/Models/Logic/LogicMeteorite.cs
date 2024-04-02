using System;

namespace Game.Model.Logics
{
    using Game.Model.Stats;

    using Units;
    using Weapons;
    using static Game.Model.Logics.Logic;

    public class LogicMeteorite: ILogic
    {
        public void Initialize(LogicDef logic)
        {
            logic.Initialize(Global.Action.Init);
            
            logic.SetState(Move.State.Init, false);

            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(1);

            logic.AddAction(Target.Action.Find)
                .AddPreconditions(Target.State.Found, false)
                .AddEffect(Target.State.Found, true)
                .Cost(2);

            logic.AddAction(Move.Action.MoveToTarget)
                .AddPreconditions(Target.State.Found, true)
                .AddEffect(Move.State.MoveDone, true)
                .Cost(1);

            logic.AddAction(Move.Action.MoveToPosition)
                .AddPreconditions(Target.State.Found, true)
                .AddEffect(Move.State.MoveDone, true)
                .Cost(10);

            logic.AddAction(Unit.Action.Attack)
                .AddPreconditions(Unit.State.WeaponInRange, false)
                .AddPreconditions(Move.State.MoveDone, true)
                .AddEffect(Unit.State.WeaponInRange, true)
                .Cost(1);

            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoal(Move.State.Init, true);
            logic.EnqueueGoal(Unit.State.WeaponInRange, true);
        }
    }
}