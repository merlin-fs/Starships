using System;
using Common.Core;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    public struct SpawnTag : IComponentData
    {
        public Entity Entity;
        public LocalTransform WorldTransform;
        public ObjectID ConfigID;
    }

    public struct Spawn : IComponentData { }
}

namespace Game.Systems
{
    using Game.Core.Defs;
    using Game.Core.Repositories;
    using Game.Model;
    using Game.Model.Stats;
    using Game.Views.Stats;

    using Unity.Collections;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class SpawnSystem : SystemBase
    {
        EntityQuery m_Query;
        BufferLookup<StatView> ViewsLookup;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SpawnTag>()
                .Build();

            ViewsLookup = GetBufferLookup<StatView>();
            RequireForUpdate(m_Query);
        }

        partial struct SpawnJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            [NativeDisableParallelForRestriction]
            public BufferLookup<StatView> ViewsLookup;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in SpawnTag spawn)
            {
                var inst = Writer.Instantiate(idx, spawn.Entity);
                
                Writer.AddComponent<Spawn>(idx, inst);
                Writer.AddComponent(idx, inst, new Move 
                { 
                    Position = spawn.WorldTransform.Position,
                    Rotation = spawn.WorldTransform.Rotation,
                });

                if (ViewsLookup.HasBuffer(entity))
                {
                    var views = ViewsLookup[entity];
                    var buff = Writer.AddBuffer<StatView>(idx, inst);
                    buff.CopyFrom(views);
                }
                Writer.DestroyEntity(idx, entity);
            }
        }

        protected override void OnUpdate()
        {
            ViewsLookup.Update(ref CheckedStateRef);
            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            Dependency = new SpawnJob()
            {
                ViewsLookup = ViewsLookup,
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, Dependency);

            system.AddJobHandleForProducer(Dependency);
        }
    }
}