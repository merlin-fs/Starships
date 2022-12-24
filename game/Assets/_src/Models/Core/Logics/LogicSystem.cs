using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using Game.Model.Weapons;

namespace Game.Model
{
    using Result = ILogic.Result;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameLogicSystemGroup), OrderFirst = true)]
    public partial class LogicSystem : SystemBase
    {
        private Dictionary<int, IJobInfo> m_Jobs;
        private int m_Index;

        private static LogicSystem m_Instance;
        public static LogicSystem Instance => m_Instance;

        EntityQuery m_Query;

        private struct Item
        {
            public Entity Entity;
            public int JobIndex;
        }

        protected override void OnCreate()
        {
            m_Jobs = new Dictionary<int, IJobInfo>();
            m_Index = 0;

            m_Instance = this;
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();

            //m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            RequireForUpdate(m_Query);
        }

        public int RegJob<T>()
            where T : unmanaged, ILogicTransition
        {
            var idx = m_Index;
            m_Jobs.Add(m_Index++, new JobInfo<T>());
            return idx;
        }

        partial struct LogicNextStateIDJob : IJobEntity
        {
            public NativeQueue<Item>.ParallelWriter Writer;

            void Execute(Entity entity, ref LogicAspect logic)//[WithChangeFilter(typeof(Logic))]
            {
                int typeIndex = logic.JobIndex;
                if (logic.Result != Result.Busy)
                {
                    var next = logic.GetNextStateID(out typeIndex);
                    logic.SetStateID(next);
                }

                if (typeIndex >= 0)
                {
                    logic.SetResult(Result.Busy, typeIndex);
                    Writer.Enqueue(new Item()
                    {
                        Entity = entity,
                        JobIndex = typeIndex,
                    });
                }
                else
                  logic.SetResult(Result.Done, typeIndex);

            }
        }

        private interface IJobInfo
        {
            JobHandle Init(Entity entity, ref SystemState state, LogicAspect logic, JobHandle dependency);
        }

        private struct JobInfo<T>: IJobInfo
            where T : unmanaged, ILogicTransition
        {
            public JobHandle Init(Entity entity, ref SystemState state, LogicAspect logic, JobHandle dependency)
            {
                T job = Activator.CreateInstance<T>();
                job.Init(entity, ref state, logic);
                return job.Schedule(dependency); ;
            }
        }

        protected override void OnUpdate()
        {
            using var queue = new NativeQueue<Item>(Allocator.TempJob);
            using var jobs = new NativeList<JobHandle>(Allocator.Temp);
            var ecb = World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var selectJob = new LogicNextStateIDJob()
            {
                Writer = queue.AsParallelWriter(),
            };
            var handle = selectJob.ScheduleParallel(m_Query, Dependency);
            handle.Complete();


            if (queue.Count == 0)
                return;

            while (queue.Count > 0)
            {
                var iter = queue.Dequeue();
                if (m_Jobs.TryGetValue(iter.JobIndex, out IJobInfo info))
                {
                    //LogicAspect.CompleteDependencyBeforeRW(ref CheckedStateRef);
                    var aspect = default(LogicAspect).CreateAspect(iter.Entity, ref CheckedStateRef, false);

                    var job = info.Init(iter.Entity, ref CheckedStateRef, aspect, handle);
                    jobs.Add(job);
                }
            }
            JobHandle.CompleteAll(jobs.AsArray());
        }
    }
}
