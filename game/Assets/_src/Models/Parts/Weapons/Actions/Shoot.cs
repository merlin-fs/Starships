using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct Shoot : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                context.SetWorldState(context.Entity, State.Shooting, false);
                context.SetWorldState(context.Entity, State.TargetLocked, false);
            }
        }
    }
}