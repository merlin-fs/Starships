using Game.Model.Logics;

using Unity.Mathematics;

namespace Game.Model
{
    public partial struct Target
    {
        public struct Find : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                var found = FindEnemy(context.Entity, context.Query.SearchTeams, 
                    (selfPosition, targetPos) => 
                        utils.SpheresIntersect(selfPosition, context.Query.Radius, targetPos, 5f, out var _), 
                    context.Entities, context.LookupLocalToWorld, context.LookupTeams, out var target);
                
                context.SetWorldState(context.Entity, State.Found, found);
                context.Writer.SetComponent(context.SortKey, context.Entity, new Target {Value = target});
            }
        }
    }
}
