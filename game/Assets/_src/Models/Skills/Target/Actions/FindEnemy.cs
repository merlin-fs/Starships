using Game.Model.Logics;

namespace Game.Model
{
    public partial struct Target
    {
        public struct Find : Logic.IAction<Context>
        {
            public void Execute(ref Context context)
            {
            }
        }
    }
}
