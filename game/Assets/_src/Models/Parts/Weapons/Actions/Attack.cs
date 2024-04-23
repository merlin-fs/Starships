using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct Attack : Logic.IAction<Context>
        {
            public void Execute(ref Context context)
            {
                context.Aspect.IncTime(context.Delta);
                if (!(context.Aspect.Time >= context.Aspect.Stat(Stats.Rate).Value)) return;
                        
                context.Logic.SetWorldState(State.Shooting, true);
                context.Aspect.ResetTime();
                context.Aspect.Shot();
                        
                if (context.Aspect.Count != 0) return;
                context.Logic.SetWorldState(State.HasAmo, false);
            }
        }
    }
}