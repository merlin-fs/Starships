using System;
using System.Collections.Concurrent;
using Unity.Entities;
using Unity.Burst;
using Common.Defs;
using System.Threading.Tasks;

using Unity.Jobs;

namespace Game.Core.Prefabs
{
    using Common.Core;
    using Common.Repositories;

    using Repositories;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class PrefabSystem : SystemBase
    {
        static PrefabSystem Instance { get; set; }

        EntityQuery m_Query;
        
        static private bool m_Done;

        ConcurrentDictionary<Entity, IDefineableContext> m_Contexts = new ConcurrentDictionary<Entity, IDefineableContext>();


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
                .WithAll<BakedPrefabTag>()
                .WithAll<BakedPrefab>()
                .WithNone<BakedEnvironment>()
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
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, Dependency);
            new DoneJob().Schedule(Dependency);
            Dependency.Complete();
            m_Contexts.Clear();

            ecb.RemoveComponent<BakedPrefabTag>(m_Query);
            //m_Done = true;
        }

        struct DoneJob : IJob
        {
            public void Execute()
            {
                m_Done = true;
            }
        }
        
        partial struct PrefabJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            readonly DIContext.Var<Repository> m_Repository;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in BakedPrefab baked)
            {
                if (!Instance.m_Contexts.TryGetValue(entity, out IDefineableContext context))
                {
                    context = new WriterContext(Writer, idx);
                    Instance.m_Contexts.TryAdd(entity, context);
                }

                var config = m_Repository.Value.FindByID(baked.ConfigID);
                config.Configurate(entity, context);
            }
        }
    }
}