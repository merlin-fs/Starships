using System;
using System.Linq;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Common.Defs;
using System.Threading.Tasks;

namespace Game.Core.Prefabs
{
    using System.Collections.Generic;

    using Repositories;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class PrefabSystem : SystemBase
    {
        EntityQuery m_Query;
        EntityQuery m_StoreQuery;

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

        [BurstDiscard]
        protected async override void OnUpdate()
        {
            if (m_StoreQuery.IsEmpty || m_Query.IsEmpty)
                return;

            m_Done = false;

            var repo = await Repositories.Instance.ConfigsAsync();
            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();

            var ecb = system.CreateCommandBuffer();
            var storeEntity = m_StoreQuery.GetSingletonEntity();
            EntityManager.AddBuffer<PreparePrefabData>(storeEntity);

            using var prefabs = new NativeList<PreparePrefabData>(this.WorldUpdateAllocator);
            using var datas = m_Query.ToComponentDataArray<PrefabTargetData>(this.WorldUpdateAllocator);
            using var uniqids = new NativeHashSet<Entity>(5, this.WorldUpdateAllocator);
            using var dst = new NativeArray<Entity>(1, Allocator.Temp);

            var writer = EntityManager;

            foreach (var data in datas)
            {
                EntityManager.SetComponentEnabled<PrefabTargetData>(data.Target, false);
                if (data.IsChild)
                    continue;

                var configs = repo.Find((i) => i.PrefabID == data.PrefabID);

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

            UnityEngine.Debug.Log($"[Prefab system] Init prefabs: {prefabs.Length}");
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
                context.SetName(prefab.Entity, config.ID.ToString());
                UnityEngine.Debug.Log($"[Prefab system] Init prefab: {config.ID}, {prefab.Entity}");
            }
            

            m_Done = true;
        }
    }
}