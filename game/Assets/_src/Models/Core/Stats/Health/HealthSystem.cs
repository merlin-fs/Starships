using System;
using Unity.Entities;
    
namespace Game.Model.Stats
{
    using Logics;

    //[UpdateInGroup(typeof(GameLogicDoneSystemGroup))]
    [UpdateInGroup(typeof(GameLogicInitSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Stat>());
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var job = new SystemJob()
            {
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct SystemJob : IJobEntity
        {
            public void Execute([EntityIndexInQuery] int idx, in Entity entity, in DynamicBuffer<Stat> stats, ref LogicAspect logic)
            {
                if (stats.TryGetStat(GlobalStat.Health, out Stat health) && health.Value <= 0)
                {
                    UnityEngine.Debug.Log($"{logic.Self} [Health system] set Destroy");
                    logic.SetAction(GlobalAction.Destroy);
                }
            }
        }
    }
}