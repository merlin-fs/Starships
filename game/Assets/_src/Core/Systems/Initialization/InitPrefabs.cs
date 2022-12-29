using System;
using UnityEngine;
using Unity.Entities;
using System.Runtime.InteropServices;
using Unity.Transforms;
using Unity.Collections;
using Common.Defs;

namespace Game.Systems
{
    /*
    [UpdateInGroup(typeof(GameSpawnSystemGroup), OrderFirst = true)]
    partial class InitPrefabSystem : SystemBase
    {
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Query = SystemAPI.QueryBuilder()
                //.WithAll<Prefab>()
                .WithAll<PrefabInitTag>()
                .WithAll<PrefabData>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities)
                .Build();
            RequireForUpdate(m_Query);
        }

        partial struct PrefabInitJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            void Execute([EntityIndexInQuery] int entityIndexInQuery, in Entity entity, in PrefabData data)
            {
                Writer.RemoveComponent<PrefabInitTag>(entityIndexInQuery, entity);

                PrefabStore.Instance.Add("Unit", data.Prefab);
            }
        }

        protected override void OnUpdate()
        {
            EntityCommandBufferSystem system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();

            var handle = new PrefabInitJob()
            {
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, Dependency);
            system.AddJobHandleForProducer(handle);
        }
    }
    */
}