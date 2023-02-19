using System;
using Unity.Burst;
using Unity.Entities;

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
                .WithAll<Stat>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        //[BurstCompile]
        partial struct PrepareStatsJob : IJobEntity
        {
            //public EntityCommandBuffer.ParallelWriter Writer;
            void Execute(in DynamicBuffer<PrepareStat> configs, ref DynamicBuffer<Stat> stats)
            {
                var repo = Repositories.Instance.ConfigsAsync().Result;
                foreach (var iter in configs)
                {
                    var config = repo.FindByID(iter.ConfigID);
                    if (config is IConfigStats statConfig)
                    {
                        statConfig.Configurate(stats);
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
                //Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);

            ecb.RemoveComponent<PrepareStat>(m_Query);
            //system.AddJobHandleForProducer(state.Dependency);
        }
    }
}