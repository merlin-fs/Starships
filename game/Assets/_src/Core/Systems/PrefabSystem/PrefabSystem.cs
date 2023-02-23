using System;
using System.Collections.Concurrent;
using Unity.Entities;
using Unity.Burst;
using Common.Defs;
using System.Threading.Tasks;

namespace Game.Core.Prefabs
{

    using Repositories;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class PrefabSystem : SystemBase
    {
        static PrefabSystem Instance { get; set; }

        EntityQuery m_Query;
        
        private bool m_Done;

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
                .WithAll<BakedPrefabData>()
                .WithOptions(EntityQueryOptions.IncludePrefab)
                .Build();

            RequireForUpdate(m_Query);
        }


        [BurstDiscard]
        protected async override void OnUpdate()
        {
            m_Done = false;
            await Repositories.Instance.ConfigsAsync();

            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            Dependency = new PrefabJob()
            {
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, Dependency);
            Dependency.Complete();
            m_Contexts.Clear();

            ecb.RemoveComponent<BakedPrefabData>(m_Query);
            m_Done = true;
        }

        partial struct PrefabJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in DynamicBuffer<BakedPrefabData> bakeds)
            {
                if (!Instance.m_Contexts.TryGetValue(entity, out IDefineableContext context))
                {
                    context = new DefExt.WriterContext(Writer, idx);
                    Instance.m_Contexts.TryAdd(entity, context);
                }

                foreach (var iter in bakeds)
                {
                    var config = Repositories.Instance.ConfigsAsync().Result.FindByID(iter.ConfigID);
                    config.Configurate(entity, context);
                }
            }
        }
    }
}