using Game.Model.Logics;

namespace Game.Model.Units
{
    public partial struct Unit
    {
        public struct Attack : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
            }
        }
    }
}