using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct TrackingTarget : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                if (context.LookupLogic[context.Entity].HasWorldState(Weapon.State.TargetLocked, true)) return;
                var rotate = new Move 
                {
                    Target = context.Weapon.Target.Value, 
                    Query = Move.QueryFlags.Rotate | Move.QueryFlags.Target,
                    Travel = context.Weapon.Stat(Weapon.Stats.RotateSpeed).Value,
                };
                context.Writer.SetComponent(context.SortKey, context.Entity, rotate);
            }
        }
    }
}