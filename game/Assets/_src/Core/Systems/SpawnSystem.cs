using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    public struct SpawnTag : IComponentData
    {
        public Entity Entity;
        public WorldTransform WorldTransform;
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

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in SpawnTag spawn)
            {
                var inst = Writer.Instantiate(idx, spawn.Entity);
                Writer.AddComponent(idx, inst, new Move 
                { 
                    Position = spawn.WorldTransform.Position,
                    Rotation = spawn.WorldTransform.Rotation,
                });
                Writer.DestroyEntity(idx, entity);
                UnityEngine.Debug.Log($"[{inst}] Inst: {spawn.WorldTransform.Position}");
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