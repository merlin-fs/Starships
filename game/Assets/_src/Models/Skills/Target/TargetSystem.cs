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
            EntityQuery m_Query;
            EntityQuery m_QueryTargets;
            ComponentLookup<LocalTransform> m_LookupTransforms;
            ComponentLookup<Team> m_LookupTeams;
            
            public void OnCreate(ref SystemState state)
            {
                m_QueryTargets = SystemAPI.QueryBuilder()
                    .WithAll<Team>()
                    .WithNone<DeadTag>()
                    .Build();

                m_Query = SystemAPI.QueryBuilder()
                    .WithAllRW<Target>()
                    .WithAspect<Logic.Aspect>()
                    .Build();

                m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Target>());
                state.RequireForUpdate(m_Query);
                m_LookupTransforms = state.GetComponentLookup<LocalTransform>(true);
                m_LookupTeams = state.GetComponentLookup<Team>(true);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupTransforms.Update(ref state);
                m_LookupTeams.Update(ref state);
                var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
                var job = new SystemJob()
                {
                    LookupTransforms = m_LookupTransforms,
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
                [ReadOnly] public ComponentLookup<LocalTransform> LookupTransforms;
                [ReadOnly] public ComponentLookup<Team> Teams;
                [ReadOnly] public NativeList<Entity> Entities;

                public void Execute([WithChangeFilter(typeof(Target))] in Entity entity, ref Target data, Logic.Aspect logic)
                {
                    if (!logic.IsCurrentAction(Action.Find)) return;

                    if (FindEnemy(data.SoughtTeams, entity, data.Radius, LookupTransforms, Teams, out data.Value, out data.Transform))
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
                    public LocalTransform Transform;
                }

                public bool FindEnemy(uint soughtTeams, Entity self, float selfRadius,
                    ComponentLookup<LocalTransform> transforms, ComponentLookup<Team> teams,
                    out Entity target, out LocalTransform transform)
                {
                    TempFindTarget find = new TempFindTarget { Entity = Entity.Null, Magnitude = float.MaxValue };
                    var CounterLock = new object();
                    var selfPosition = transforms[self].Position;
                    var entities = Entities;

                    Parallel.For(0, entities.Length, (i) =>
                    {
                        var target = entities[i];
                        if (!teams.HasComponent(target))
                            return;

                        var team = teams[target];
                        if ((team.SelfTeam & soughtTeams) == 0)
                            return;

                        var targetPos = transforms[target].Position;

                        var magnitude = (selfPosition - targetPos).magnitude();

                        if (magnitude < find.Magnitude &&
                            utils.SpheresIntersect(selfPosition, selfRadius, targetPos, 5f, out float3 vector))
                        {
                            lock (CounterLock)
                            {
                                find.Magnitude = magnitude;
                                find.Entity = target;
                                find.Transform = transforms[target];
                            }
                        }
                    });
                    target = find.Entity;
                    transform = find.Transform;
                    return target != Entity.Null;
                }
            }
        }
    }
}
