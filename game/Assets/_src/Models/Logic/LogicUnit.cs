using System;

namespace Game.Model.Logics
{
    using static Game.Model.Logics.Logic;
    public class LogicUnit: ILogic
    {
        public void Init(LogicDef logic)
        {
            logic.SetInitializeState(Move.State.Init, false);

            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(0);

            logic.EnqueueGoal(Move.State.Init, true);

        }
    }
}