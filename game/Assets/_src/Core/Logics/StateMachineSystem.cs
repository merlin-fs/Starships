using System;
using Unity.Entities;

namespace Game.Model.Logics
{
    using Stats;

    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderLast = true)]
    public partial class StateMachineSystem : SystemBase
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

        partial struct StateMachineJob : IJobEntity
        {
            void Execute([WithChangeFilter(typeof(Logic))] ref LogicAspect logic)
            {
                if (!logic.IsWork)
                {
                    var next = logic.GetNextState();
                    logic.SetState(next);
                }
            }
        }

        protected override void OnUpdate()
        {
            var selectJob = new StateMachineJob()
            {
            };
            var handle = selectJob.ScheduleParallel(m_Query, Dependency);
            handle.Complete();
        }
    }
}
