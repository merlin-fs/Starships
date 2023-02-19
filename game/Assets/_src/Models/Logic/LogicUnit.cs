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
            logic.SetInitializeState(Move.State.Init, false);
            logic.SetInitializeState(Unit.State.WeaponsActive, false);

            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(0);

            logic.AddAction(Unit.Action.ActiveWeapons)
                .AddPreconditions(Unit.State.WeaponsActive, false)
                .AddEffect(Unit.State.WeaponsActive, true)
                .Cost(0);

            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .AddEffect(Global.State.Dead, true)
                .Cost(0);

            logic.EnqueueGoal(Move.State.Init, true);
            logic.EnqueueGoal(Unit.State.WeaponsActive, true);
        }
    }
}