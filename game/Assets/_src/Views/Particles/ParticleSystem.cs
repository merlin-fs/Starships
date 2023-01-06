using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Game.Model.Logics;
using Game.Model.Weapons;
using Game.Model.Stats;

namespace Game.Views
{
    [UpdateInGroup(typeof(GamePresentationSystemGroup))]
    public partial class TestParticleSystem : SystemBase
    {
        private EntityCommandBufferSystem m_CommandBuffer;
        private EntityQuery m_Query;
        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Particle>()
                .WithNone<ParticleView>()
                .WithNone<DeadTag>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Particle>());
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            var childs = GetBufferLookup<Child>(true);
            var views = GetComponentLookup<ParticleView>(true);
            //var particles = GetComponentLookup<ParticleSystem>(true);

            using var entities = m_Query.ToEntityArray(Allocator.Temp);
            using var tags = m_Query.ToComponentDataArray<Particle>(Allocator.Temp);
            var writer = m_CommandBuffer.CreateCommandBuffer();

            for (int i = 0; i < entities.Length; i++)
            {
                var iter = entities[i];
                
                RecursiveChilds(iter, childs, (child) =>
                {
                    var view = views.GetRefROOptional(child);

                    if (view.IsValid && view.ValueRO.ID == tags[i].ID)
                    {
                        //var particle = particles.GetRefROOptional(child);
                        if (EntityManager.HasComponent<ParticleSystem>(child))
                        {
                            var ps = EntityManager.GetComponentObject<ParticleSystem>(child);
                            ps.Play();
                        }
                        if (EntityManager.HasComponent<AudioSource>(child))
                        {
                            var audio = EntityManager.GetComponentObject<AudioSource>(child);
                            audio.Play();
                        }
                    }
                });
                writer.RemoveComponent<Particle>(iter);
                //writer.SetComponent<Particle>(iter, "");
            }
        }

        void RecursiveChilds(Entity entity, BufferLookup<Child> childs, Action<Entity> action)
        {
            action?.Invoke(entity);
            if (!childs.HasBuffer(entity))
                return;
            var child = childs[entity];
            for (var i = 0; i < child.Length; ++i)
            {
                var iter = child[i].Value;
                RecursiveChilds(iter, childs, action);
            }
        }
    }
}