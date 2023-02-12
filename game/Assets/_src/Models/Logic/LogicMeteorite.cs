using System;

namespace Game.Model.Logics
{
    using Weapons;
    using static Game.Model.Logics.Logic;
    public class LogicMeteorite: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeState(Weapon.State.NoAmmo, true);
            logic.SetInitializeState(Move.State.Init, false);

            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(0);

            logic.AddAction(Target.Action.Find)
                .AddPreconditions(Target.State.Found, false)
                .AddEffect(Target.State.Found, true)
                .Cost(2);

            logic.AddAction(Move.Action.MoveTo)
                .AddEffect(Move.State.MoveDone, false)
                .AddPreconditions(Move.State.Init, true)
                .AddEffect(Move.State.MoveDone, true)
                .Cost(1);

            logic.AddGoal(Move.State.MoveDone, true);

        }
    }
}