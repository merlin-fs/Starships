using Game.Model.Logics;

using Unity.Mathematics;

namespace Game.Model
{
    public partial struct Move
    {
        public struct WeaponTargetLocked : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                var target = context.LookupWorldTransform.Transform(context.Entity);
                var dot = math.abs(math.dot(context.Move.Rotation, target.Rotation)); 
                if (dot > 0.9) 
                    context.SetWorldState(context.Entity, Weapons.Weapon.State.TargetLocked, true);
            }
        }
    }
}
