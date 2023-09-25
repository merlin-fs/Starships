using System;

namespace Game.Model.Logics
{
    using Game.Model.Stats;

    using Units;
    using Weapons;
    using static Game.Model.Logics.Logic;

    public class LogicPA_Warior: ILogic
    {
        public void Initialize(LogicDef logic)
        {
            logic.Initialize(Global.Action.Init);
            
            logic.SetState(Move.State.Init, true);
            logic.SetState(Unit.State.WeaponInRange, false);

            /*
            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(1);
            */

            logic.AddAction(Target.Action.Find)
                .AddPreconditions(Target.State.Found, false)
                .AddEffect(Target.State.Found, true)
                .Cost(2);

            logic.AddAction(Move.Action.FindPath)
                .AddPreconditions(Target.State.Found, true)
                .AddEffect(Move.State.PathFound, true)
                .Cost(1);

            logic.AddAction(Move.Action.MoveToPoint)
                .AddPreconditions(Move.State.PathFound, true)
                .AddPreconditions(Unit.State.WeaponInRange, false)
                .AddEffect(Move.State.MoveDone, true)
                .AddEffect(Unit.State.WeaponInRange, true)
                .Cost(1);
            
            logic.AddAction(Unit.Action.Attack)
                .AddPreconditions(Move.State.MoveDone, true)
                .AddPreconditions(Unit.State.WeaponInRange, true)
                .AddEffect(Target.State.Dead, true)
                .Cost(1);

            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoal(Move.State.Init, true);
            logic.EnqueueGoal(Unit.State.WeaponInRange, true);
            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}