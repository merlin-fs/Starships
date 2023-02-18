using System;

namespace Game.Model.Logics
{
    using Units;
    using Weapons;
    using static Game.Model.Logics.Logic;

    public class LogicMeteorite: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeState(Move.State.Init, false);

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

            logic.AddAction(Unit.Action.ActiveWeapons)
                .AddPreconditions(Unit.State.WeaponsActive, false)
                .AddPreconditions(Move.State.MoveDone, true)
                .AddEffect(Unit.State.WeaponsActive, true)
                .Cost(1);

            logic.EnqueueGoal(Move.State.Init, true);
            logic.EnqueueGoal(Unit.State.WeaponsActive, true);
        }
    }
}