using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

namespace Game.Model
{
    using Stats;
    using Logics;

    //TODO: нужно переделать на Job`ы (заменить Parallel.For)
    public partial struct Target
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct TargetSystem : ISystem
        {
            private EntityQuery m_Query;
            private EntityQuery m_QueryTargets;
            private ComponentLookup<LocalToWorld> m_LookupLocalToWorld;
            private ComponentLookup<Team> m_LookupTeams;
            
            public void OnCreate(ref SystemState state)
            {
                m_QueryTargets = SystemAPI.QueryBuilder()
                    .WithAll<Team>()
                    .WithNone<DeadTag>()
                    .Build();

                m_Query = SystemAPI.QueryBuilder()
                    .WithAllRW<Target>()
                    .WithAll<Query>()
                    .WithAspect<Logic.Aspect>()
                    .Build();

                //m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Target>());
                state.RequireForUpdate(m_Query);
                m_LookupLocalToWorld = state.GetComponentLookup<LocalToWorld>(true);
                m_LookupTeams = state.GetComponentLookup<Team>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupLocalToWorld.Update(ref state);
                m_LookupTeams.Update(ref state);
                var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
                var job = new SystemJob()
                {
                    LookupLocalToWorld = m_LookupLocalToWorld,
                    Teams = m_LookupTeams,
                    Entities = entities,
                    Delta = SystemAPI.Time.DeltaTime,
                };
                state.Dependency = job.ScheduleParallel(m_Query, handle);

                entities.Dispose(state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                [ReadOnly] public ComponentLookup<LocalToWorld> LookupLocalToWorld;
                [ReadOnly] public ComponentLookup<Team> Teams;
                [ReadOnly] public NativeList<Entity> Entities;

                private void Execute([WithChangeFilter(typeof(Target))] in Entity entity, ref Target result, 
                    in Query query, Logic.Aspect logic)
                {
                    if (!logic.IsCurrentAction<Find>()) return;

                    if (FindEnemy(query.SearchTeams, entity, query.Radius, LookupLocalToWorld, Teams, out result.Value))
                    {
                        //var selfPosition = LookupTransforms[logic.Self].Position;
                        //UnityEngine.Debug.Log($"{logic.Self} [Target] found: self - {selfPosition}, team - {data.SoughtTeams}, target - {data.Value}");
                        logic.SetWorldState(State.Found, true);
                    }
                    else
                    {
                        //var selfPosition = LookupTransforms[logic.Self].Position;
                        //UnityEngine.Debug.Log($"{logic.Self} [Target] not found: self - {selfPosition}, team - {data.SoughtTeams}");
                        logic.SetWorldState(State.Found, false);
                    }
                }

                struct TempFindTarget
                {
                    public Entity Entity;
                    public float Magnitude;
                }

                private bool FindEnemy(uint soughtTeams, Entity self, float selfRadius,
                    ComponentLookup<LocalToWorld> transforms, ComponentLookup<Team> teams, out Entity target)
                {
                    TempFindTarget find = new TempFindTarget { Entity = Entity.Null, Magnitude = float.MaxValue };
                    var selfPosition = transforms[self].Position;
                    var entities = Entities;

                    foreach (var candidate in entities)
                    {
                        var team = teams[candidate];
                        if ((team.SelfTeam & soughtTeams) == 0)
                            continue;

                        var targetPos = transforms[candidate].Position;
                        var magnitude = (selfPosition - targetPos).magnitude();

                        if (magnitude < find.Magnitude &&
                            utils.SpheresIntersect(selfPosition, selfRadius, targetPos, 5f, out float3 vector))
                        {
                            find.Magnitude = magnitude;
                            find.Entity = candidate;
                        }
                    };

                    target = find.Entity;
                    return target != Entity.Null;
                }
            }
        }
    }
}
