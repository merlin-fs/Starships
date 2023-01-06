using System;
using Game.Model.Stats;

using Unity.Entities;

namespace Game.Model.Logics
{
    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderLast = true)]
    public partial class LogicSystem : SystemBase
    {
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            RequireForUpdate(m_Query);
        }

        partial struct LogicJob : IJobEntity
        {
            void Execute([WithChangeFilter(typeof(Logic))] ref LogicAspect logic)
            {
                if (!logic.IsValid)
                    return;

                if (!logic.IsWork)
                {
                    var next = logic.GetNextStateID();
                    logic.SetStateID(next);
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
