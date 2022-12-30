using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

namespace Game.Model
{
    using Logics;
    using Result = Logics.Logic.Result;

    [UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    public partial struct TargetSystem : ISystem
    {
        EntityQuery m_Query;
               
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Target>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var entities = m_Query.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);

            var job = new Job()
            {
                Transforms = state.GetComponentLookup<LocalTransform>(true),
                Teams = state.GetComponentLookup<Team>(true),
                Entities = entities,
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, handle);

            entities.Dispose(state.Dependency);
        }

        partial struct Job : IJobEntity
        {
            public float Delta;
            [ReadOnly]
            public ComponentLookup<LocalTransform> Transforms;
            [ReadOnly]
            public ComponentLookup<Team> Teams;
            [ReadOnly]
            public NativeList<Entity> Entities;

            void Execute(in Entity entity, ref Target data, ref LogicAspect logic)
            {
                if (logic.Equals(Target.State.Find))
                {
                    if (FindEnemy(data.SoughtTeams, entity, 5f, Transforms, Teams, out data.Value))
                        logic.SetResult(Result.Done);
                    else
                        logic.SetResult(Result.Error);
                }
            }

            struct TempFindTarget
            {
                public Entity Entity;
                public float Magnitude;
            }

            public bool FindEnemy(uint soughtTeams, Entity self, float selfRadius,
                ComponentLookup<LocalTransform> transforms, ComponentLookup<Team> teams, out Entity target)
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
                        }
                    }
                });
                target = find.Entity;
                return target != Entity.Null;
            }
        }
    }
}
