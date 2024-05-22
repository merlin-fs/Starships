using Common.Defs;

using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct Reload : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                context.Weapon.IncTime(context.Delta);  
                context.Writer.SetComponent(context.SortKey, context.Entity, new Logic.ChangeTag());

                if (!(context.Weapon.Time >= context.Weapon.Stat(Stats.ReloadTime).Value)) return;
                        
                context.Weapon.ResetTime();

                //TODO: нужно перенести получение кол. патронов...
                //if (!logic.HasWorldState(State.HasAmo, true)) return;
                            
                var count = (int)context.Weapon.Stat(Stats.ClipSize).Value;
                context.SetWorldState(context.Entity, State.HasAmo, 
                    context.Weapon.Reload(
                        new WriterContext(context.Writer, context.SortKey), 
                        count, context.ObjectRepository));
            }
        }
    }
}