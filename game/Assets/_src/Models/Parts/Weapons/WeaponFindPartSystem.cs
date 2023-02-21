using System;
using Unity.Entities;
using Unity.Collections;

namespace Game.Model.Weapons
{
    using Logics;

    public partial struct Weapon
    {
        [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
        public partial struct WeaponFindPartSystem : ISystem
        {
            EntityQuery m_Query;
            ComponentLookup<Team> m_LookupTeams;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Weapon>()
                    .WithAll<Logic>()
                    .Build();
                state.RequireForUpdate(m_Query);
                m_LookupTeams = state.GetComponentLookup<Team>(false);
            }

            public void OnDestroy(ref SystemState state) { }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupTeams.Update(ref state);
                var job = new SystemJob()
                {
                    Teams = m_LookupTeams,
                };
                state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                [ReadOnly] public ComponentLookup<Team> Teams;
                public void Execute(ref WeaponAspect weapon, in Logic.Aspect logic)
                {
                    if (logic.IsCurrentAction(Target.Action.Find))
                    {
                        //UnityEngine.Debug.Log($"{logic.Self} [Logic part] FindOfWeaponTarget set teams {weapon.Unit}");
                        var target = weapon.Target;
                        target.SoughtTeams = Teams[weapon.Root].EnemyTeams;
                        target.Radius = weapon.Stat(Stats.Range).Value;
                        weapon.Target = target;
                    }
                }
            }
        }
    }
}