using System;
using Unity.Entities;

namespace Game.Core.Prefabs
{
    using Repositories;

    using Unity.Burst;
    using Unity.Collections;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class SpawnSystem : SystemBase
    {
        EntityQuery m_Query;
        EntityQuery m_StoreQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<PrefabTargetData>()
                .WithOptions(EntityQueryOptions.IncludePrefab)
                .Build();
            
            m_StoreQuery = SystemAPI.QueryBuilder()
                .WithAll<PrefabData>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IgnoreComponentEnabledState)
                .Build();

            RequireForUpdate(m_Query);
        }

        partial struct PrepareJob : IJobEntity
        {
            public Entity Store;
            public EntityCommandBuffer.ParallelWriter Writer;
            //public EntityManager Writer;


            void Execute([EntityIndexInQuery] int idx, in Entity entity, PrefabTargetData data)
            {
                if (data.IsChild)
                    return;

                var repo = Repositories.Instance.ConfigsAsync().Result;

                var configs = repo.Find((i) => i.PrefabID == data.PrefabID);

                foreach (var config in configs)
                {
                    var prefab = Writer.Instantiate(idx, entity);
                    Writer.AddComponent<Prefab>(idx, prefab);

                    Writer.AppendToBuffer(idx, Store, new PreparePrefabData()
                    {
                        Entity = prefab,
                        ID = config.ID,
                    });

                    Writer.RemoveComponent<PrefabTargetData>(idx, prefab);
                    Writer.RemoveComponent<PrefabTargetData>(idx, entity);
                }
            }
        }

        [BurstDiscard]
        protected async override void OnUpdate()
        {
            if (m_StoreQuery.IsEmpty)
                return;

            await Repositories.Instance.ConfigsAsync();

            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();

            var ecb = system.CreateCommandBuffer();
            var store = m_StoreQuery.GetSingletonEntity();// SystemAPI.GetSingletonEntity<PrefabData>();
            EntityManager.AddBuffer<PreparePrefabData>(store);

            Dependency = new PrepareJob()
            {
                Store = store,
                Writer = ecb.AsParallelWriter(),

            }
            .Schedule(m_Query, Dependency);
            //.ScheduleParallel(m_Query, Dependency);
            //system.AddJobHandleForProducer(Dependency);
            Dependency.Complete();
            ecb.RemoveComponent<PrefabData>(store);
            //EntityManager.DestroyEntity(m_Query);
        }
    }
}