using System;
using Unity.Entities;

namespace Game.Model
{
    public struct SpawnTag : IComponentData
    {
        public Entity Entity;
    }
}

namespace Game.Systems
{
    using Game.Model;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class SpawnSystem : SystemBase
    {
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SpawnTag>()
                .Build();
            RequireForUpdate(m_Query);
        }

        partial struct SpawnJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int entityIndexInQuery, in Entity entity, in SpawnTag spawn)
            {
                UnityEngine.Debug.Log($"try spawn: {spawn.Entity}");
                Writer.Instantiate(entityIndexInQuery, spawn.Entity);
                Writer.DestroyEntity(entityIndexInQuery, entity);
            }
        }

        protected override void OnUpdate()
        {
            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            Dependency = new SpawnJob()
            {
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, Dependency);

            system.AddJobHandleForProducer(Dependency);
        }
    }
}