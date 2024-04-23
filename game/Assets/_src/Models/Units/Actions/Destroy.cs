using Game.Model.Logics;
using Game.Model.Stats;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public struct Destroy : Logic.IAction<Context>
        {
            public void Execute(ref Context context)
            {
                context.Logic.SetWorldState(Global.State.Dead, true);
            }
        }
    }
}