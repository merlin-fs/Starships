using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct Attack : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                context.Weapon.IncTime(context.Delta);
                context.Writer.SetComponent(context.SortKey, context.Entity, new Logic.ChangeTag());
                
                if (!(context.Weapon.Time >= context.Weapon.Stat(Stats.Rate).Value)) return;
                        
                context.SetWorldState(context.Entity, State.Shooting, true);
                context.Weapon.ResetTime();
                context.Weapon.Shot();
                        
                if (context.Weapon.Count != 0) return;
                context.SetWorldState(context.Entity, State.HasAmo, false);
            }
        }
    }
}