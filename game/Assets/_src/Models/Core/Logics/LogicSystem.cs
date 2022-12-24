using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using Game.Model.Weapons;

namespace Game.Model
{
    using Result = ILogic.Result;

    [UpdateInGroup(typeof(GameLogicSystemGroup), OrderFirst = true)]
    public partial class LogicSystem : SystemBase
    {
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            RequireForUpdate(m_Query);
        }

        partial struct LogicJob : IJobEntity
        {
            void Execute([WithChangeFilter(typeof(Logic))] ref LogicAspect logic)
            {
                if (logic.Result != Result.Busy)
                {
                    var next = logic.GetNextStateID();
                    logic.SetStateID(next);
                    logic.SetResult(Result.Busy);
                }
            }
        }

        protected override void OnUpdate()
        {
            var selectJob = new LogicJob()
            {
            };
            var handle = selectJob.ScheduleParallel(m_Query, Dependency);
            handle.Complete();
        }
    }
}
