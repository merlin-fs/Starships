using System;
using Game.Model.Stats;
using Game.Model.Units;
using static Game.Model.Logics.Logic;

namespace Game.Model.Logics
{
    public class LogicPA_Warior: ILogic
    {
        public void Initialize(LogicDef logic)
        {
            /*
            1. Находит цель
            2. выбирает точку возле цели. Цель должна быть в радиусе действия оружия. 
            3. перемещается к выбранной точке
            4. Кокда цель будет в радиусе действия оружия - останавливается 
            5. Активирует оружие
            6. начинает атаку.
            7. повтор...
            */
        
            logic.Initialize(Global.Action.Init);
            
            logic.SetState(Move.State.Init, true);
            logic.SetState(Unit.State.WeaponInRange, false);

            /*
            logic.AddAction(Move.Action.Init)
                .AddPreconditions(Move.State.Init, false)
                .AddEffect(Move.State.Init, true)
                .Cost(1);
            */

            //поиск цели
            logic.AddAction(Target.Action.Find)
                .AddPreconditions(Target.State.Found, false)
                .AddEffect(Target.State.Found, true)
                .Cost(2);
            //установка точки возле цели, в радиусе действия оружия.
            logic.CustomAction<Unit, Unit.FindPathRadius>()
                .AfterChangeState(Target.State.Found, true);

            //поиск пути к выбранной точке
            logic.AddAction(Move.Action.FindPath)
                .AddPreconditions(Target.State.Found, true)
                .AddEffect(Move.State.PathFound, true)
                .Cost(1);

            //перемещение к выбранной точке, 
            logic.AddAction(Move.Action.MoveToPoint)
                .AddPreconditions(Move.State.PathFound, true)
                .AddPreconditions(Unit.State.WeaponInRange, false)
                .AddEffect(Move.State.MoveDone, true)
                .AddEffect(Unit.State.WeaponInRange, true)
                .Cost(1);
            //атака
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