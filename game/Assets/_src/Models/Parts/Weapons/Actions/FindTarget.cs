using Game.Model.Logics;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        public struct FindTarget : Logic.IAction<Context>
        {
            public void Execute(Context context)
            {
                context.Writer.SetComponent(context.SortKey, context.Entity, 
                    new Target.Query 
                    {
                        Radius = context.Weapon.Stat(Stats.Range).Value, 
                        SearchTeams = context.LookupTeam[context.Weapon.Root].EnemyTeams,
                    });
            }
        }
    }
}