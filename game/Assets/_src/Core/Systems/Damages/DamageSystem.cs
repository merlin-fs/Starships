using System;
using System.Collections.Concurrent;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;

namespace Game.Model.Weapons
{
    public partial struct Damage
    {
        struct Data : IComponentData
        {
            public Entity Sender;
            public WorldTransform SenderTransform;
            public Target Target;
            public Bullet Bullet;
            public float Value;
        }

        public static void Apply(Entity entity, WorldTransform SenderTransform, Target target, Bullet bullet, float value)
        {
            DamageSystem.Damage(entity, SenderTransform, target, bullet, value);
        }


        [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
        [UpdateBefore(typeof(DamageProcessSystem))]
        partial class DamageSystem : SystemBase
        {
            static ConcurrentQueue<Data> m_Queue;

            protected override void OnCreate()
            {
                m_Queue = new ConcurrentQueue<Data>();
            }

            public static void Damage(Entity entity, WorldTransform SenderTransform, Target target, Bullet bullet, float value)
            {
                m_Queue.Enqueue(new Data
                {
                    Sender = entity,
                    SenderTransform = SenderTransform,
                    Value = value,
                    Target = target,
                    Bullet = bullet,
                });
            }

            struct SystemJob : IJobParallelFor
            {
                public EntityCommandBuffer.ParallelWriter Writer;
                public NativeArray<Data> Items;
                public void Execute(int index)
                {
                    var entity = Writer.CreateEntity(index);
                    Writer.AddComponent(index, entity, Items[index]);
                }
            }

            protected override void OnUpdate()
            {
                if (m_Queue.Count == 0)
                    return;
                var items = new NativeArray<Data>(m_Queue.ToArray(), Allocator.TempJob);
                m_Queue.Clear();

                var jobAdd = new SystemJob
                {
                    Writer = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GameSpawnSystemCommandBufferSystem>()
                        .CreateCommandBuffer()
                        .AsParallelWriter(),
                    Items = items,
                }.Schedule(items.Length, 5, Dependency);
                items.Dispose(jobAdd);
                jobAdd.Complete();
            }
        }
    }
}
