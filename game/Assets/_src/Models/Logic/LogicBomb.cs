using System;

namespace Game.Model.Logics
{
    using Game.Model.Stats;

    using Weapons;
    using static Game.Model.Logics.Logic;
    
    public class LogicBomb: ILogic
    {
        public void Initialize(LogicDef logic)
        {
            logic.Initialize(Global.Action.Init);
                
            logic.AddAction(Weapon.Action.Reload)
                .AddPreconditions(Weapon.State.HasAmo, false)
                .AddEffect(Weapon.State.HasAmo, true)
                .Cost(1);

            logic.AddAction(Weapon.Action.Attack)
                .AddPreconditions(Weapon.State.Active, true)
                .AddPreconditions(Weapon.State.HasAmo, true)
                .AddEffect(Target.State.Dead, true)
                .Cost(4);

            logic.AddAction(Global.Action.Destroy)
                .AddPreconditions(Global.State.Dead, false)
                .Cost(0);

            logic.EnqueueGoalRepeat(Target.State.Dead, true);
        }
    }
}