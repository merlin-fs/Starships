using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace Game.Model
{
    using Stats;
    using Logics;

    public partial struct Target
    {
        [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
        public partial struct CleanupSystem : ISystem
        {
            EntityQuery m_Query;
            EntityQuery m_QueryTargets;

            public void OnCreate(ref SystemState state)
            {
                m_QueryTargets = SystemAPI.QueryBuilder()
                    .WithAllRW<Target>()
                    .WithAspect<Logic.Aspect>()
                    .Build();

                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<DeadTag>()
                    .Build();

                state.RequireForUpdate(m_Query);
            }

            public void OnDestroy(ref SystemState state) { }

            public void OnUpdate(ref SystemState state)
            {
                var entities = m_Query.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
                state.Dependency = new Job
                {
                    Entities = entities,
                }.ScheduleParallel(m_QueryTargets, handle);
                entities.Dispose(state.Dependency);
            }

            partial struct Job : IJobEntity
            {
                [ReadOnly] public NativeList<Entity> Entities;

                void Execute(ref Target data, Logic.Aspect logic)
                {
                    foreach (var iter in Entities)
                    {
                        if (iter == data.Value)
                        {
                            UnityEngine.Debug.Log($"{logic.Self} [Target] clear target {iter}");
                            data.Value = Entity.Null;
                            logic.SetWorldState(State.Found, false);
                        }
                    }
                }
            }
        }
    }
}
