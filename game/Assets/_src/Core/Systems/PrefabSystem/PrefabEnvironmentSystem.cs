using System;
using System.Threading.Tasks;
using System.Linq;
using Unity.Entities;
using Unity.Burst;
using Common.Core;
using Common.Defs;

namespace Game.Core.Prefabs
{
    using Repositories;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct PrefabEnvironmentSystem : ISystem
    {
        EntityQuery m_Query;
        private static bool m_Done;

        public Task<bool> IsDone()
        {
            return Task.Run(() =>
            {
                while (!m_Done)
                    Task.Yield();
                return m_Done;
            });
        }

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<BakedPrefabEnvironment>()
                .WithAll<BakedPrefabLabel>()
                .WithOptions(EntityQueryOptions.IncludePrefab)
                .Build();

            state.RequireForUpdate(m_Query);
        }


        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            //state.GetDynamicComponentTypeHandle()
            m_Done = false;
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
            
            state.Dependency = new PrefabJob()
            {
            }.ScheduleParallel(m_Query, state.Dependency);
            ecb.RemoveComponent<BakedPrefabEnvironment>(m_Query);
            state.CompleteDependency();
            m_Done = true;
        }

        partial struct PrefabJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            readonly DIContext.Var<Repository> m_Repository;

            public void Execute(in Entity entity, in BakedPrefabEnvironment baked, in DynamicBuffer<BakedPrefabLabel> labels)
            {
                //this.__TypeHandle
                var localLabels = labels.AsNativeArray().ToArray()
                    .Select(i => i.Label.ToString());
                m_Repository.Value.Insert(baked.ConfigID, new Config(baked.ConfigID, entity), localLabels.ToArray());
            }
        }
    }
}