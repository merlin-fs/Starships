using System;
using Unity.Entities;
using Common.Core;

namespace Game.Model.Stats
{
    using Core.Defs;
    using Core.Repositories;
    
    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
    public partial struct StatPrepareSystem : ISystem
    {
        private EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<PrepareStat>()
                .WithAllRW<Stat>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        partial struct PrepareStatsJob : IJobEntity
        {
            private static ObjectRepository Repository => Inject<ObjectRepository>.Value;

            void Execute(in DynamicBuffer<PrepareStat> configs, ref DynamicBuffer<Stat> stats)
            {
                var repo = Repository;
                foreach (var iter in configs)
                {
                    var config = repo.FindByID(iter.ConfigID);
                    if (config is IConfigStats statConfig)
                    {
                        statConfig.Configure(stats);
                    }
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var system = state.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            state.Dependency = new PrepareStatsJob()
            {
            }.ScheduleParallel(m_Query, state.Dependency);

            ecb.RemoveComponent<PrepareStat>(m_Query, EntityQueryCaptureMode.AtPlayback);
        }
    }
}