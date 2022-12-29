using System;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Common.Defs;

namespace Game.Core.Prefabs
{
    using System.Linq;

    using Repositories;

    using Unity.Transforms;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    //[UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial class PrefabSystem : SystemBase
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

        /*
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
        */

        [BurstDiscard]
        protected async override void OnUpdate()
        {
            if (m_StoreQuery.IsEmpty || m_Query.IsEmpty)
                return;

            var repo = await Repositories.Instance.ConfigsAsync();
            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();

            UnityEngine.Debug.Log($"[Prefab system] Init");

            var ecb = system.CreateCommandBuffer();
            var storeEntity = m_StoreQuery.GetSingletonEntity();
            EntityManager.AddBuffer<PreparePrefabData>(storeEntity);

            using var prefabs = new NativeList<PreparePrefabData>(this.WorldUpdateAllocator);
            using var datas = m_Query.ToComponentDataArray<PrefabTargetData>(this.WorldUpdateAllocator);
            using var uniqids = new NativeHashSet<Entity>(5, this.WorldUpdateAllocator);
            using var dst = new NativeArray<Entity>(1, Allocator.Temp);

            var writer = EntityManager;
            UnityEngine.Debug.Log($"[Prefab system] found {datas.Length} PrefabTargetData");

            foreach (var data in datas)
            {
                EntityManager.SetComponentEnabled<PrefabTargetData>(data.Target, false);
                if (data.IsChild)
                    continue;

                var configs = repo.Find((i) => i.PrefabID == data.PrefabID);

                UnityEngine.Debug.Log($"[Prefab system] {data.PrefabID} configs: {configs.Count()}");

                foreach (var config in configs)
                {
                    var prefab = data.Target;
                    if (uniqids.Contains(data.Target))
                    {
                        prefab = EntityManager.Instantiate(data.Target);
                        var h = GetBufferTypeHandle<LinkedEntityGroup>(true);
                        var children = writer.GetBuffer<LinkedEntityGroup>(prefab);
                        
                        foreach (var child in children)
                            ecb.AddComponent<Prefab>(child.Value);
                        
                    }
                    uniqids.Add(data.Target);

                    var prefabData = new PreparePrefabData()
                    {
                        Entity = prefab,
                        ID = config.ID,
                        PrefabID = data.PrefabID,
                    };
                    prefabs.Add(prefabData);
                    ecb.AppendToBuffer(storeEntity, prefabData);
                }
            }

            foreach (var prefab in prefabs)
            {
                using var map = new NativeHashMap<Hash128, Entity>(5, this.WorldUpdateAllocator);
                var config = repo.FindByID(prefab.ID);
                var group = writer.GetBuffer<LinkedEntityGroup>(prefab.Entity).AsNativeArray();
                foreach (var child in group)
                {
                    if (writer.HasComponent<PrefabTargetData>(child.Value))
                    {
                        var data = writer.GetComponentData<PrefabTargetData>(child.Value);
                        map.Add(data.PrefabID, child.Value);
                    }
                }
                var context = new DefExt.CommandBufferContext(ecb, map);
                config.Configurate(prefab.Entity, context);
                UnityEngine.Debug.Log($"[Prefab system] prefab {prefab.ID}, {config.ID}, {config.Prefab}");
            }
            /*
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
            */
        }
    }
}