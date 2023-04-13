using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Common.Core;

namespace Game.Model
{
    public struct NewSpawnWorld : IComponentData
    {
        public Entity Prefab;
        public LocalTransform WorldTransform;
    }

    public struct NewSpawnMap : IComponentData
    {
        public Entity Prefab;
        public int2 Position;
    }

    public struct SpawnComponent : IBufferElementData
    {
        public ComponentType ComponentType;
        
        public static implicit operator SpawnComponent(ComponentType componentType) => new SpawnComponent { ComponentType = componentType };
    }

    public struct SpawnEventTag : IComponentData { }

    public struct SpawnTag : IComponentData { }
}

namespace Game.Systems
{
    using Model;
    using Views.Stats;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class SpawnSystem : SystemBase
    {
        EntityQuery m_Query;
        BufferLookup<StatView> ViewsLookup;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<NewSpawnWorld>()
                .Build();

            ViewsLookup = GetBufferLookup<StatView>();
            RequireForUpdate(m_Query);
        }

        partial struct SpawnJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            [NativeDisableParallelForRestriction]
            public BufferLookup<StatView> ViewsLookup;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in NewSpawnWorld spawn)
            {
                var inst = Writer.Instantiate(idx, spawn.Prefab);
                
                Writer.AddComponent<SpawnTag>(idx, inst);
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