using System;
using Unity.Entities;
using Unity.Burst;
using Common.Defs;
using System.Threading.Tasks;

namespace Game.Core.Prefabs
{
    using Repositories;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class PrefabEnvironmentSystem : SystemBase
    {
        static PrefabEnvironmentSystem Instance { get; set; }

        EntityQuery m_Query;
        
        private bool m_Done;

        public Task<bool> IsDone()
        {
            return Task.Run(() =>
            {
                while (!m_Done)
                    Task.Yield();
                return m_Done;
            });
        }

        protected override void OnCreate()
        {
            Instance = this;
            base.OnCreate();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<BakedPrefabEnvironmentData>()
                .WithOptions(EntityQueryOptions.IncludePrefab)
                .Build();

            RequireForUpdate(m_Query);
        }


        [BurstDiscard]
        protected override void OnUpdate()
        {
            m_Done = false;
            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            Dependency = new PrefabJob()
            {
            }.ScheduleParallel(m_Query, Dependency);
            Dependency.Complete();
            ecb.RemoveComponent<BakedPrefabEnvironmentData>(m_Query);
            m_Done = true;
        }

        partial struct PrefabJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute(in Entity entity, in BakedPrefabEnvironmentData baked)
            {
                Repositories.Instance.GetRepo("floor").Insert(new Config(baked.ConfigID, entity));
            }
        }
    }
}