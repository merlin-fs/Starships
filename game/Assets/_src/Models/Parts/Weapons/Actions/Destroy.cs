using Game.Model.Logics;
using Game.Model.Stats;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct Destroy : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                context.SetWorldState(context.Entity, Global.State.Dead, true);
            }
        }
    }
}