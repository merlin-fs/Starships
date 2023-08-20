using System;

using Game.Core;
using Game.Core.Spawns;

using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace Game.Views.Stats
{
    using Model.Stats;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial struct StatViewSystem : ISystem
    {
        private EntityQuery m_QueryInstall;
        private EntityQuery m_QueryUpdate;
        private EntityQuery m_QueryRemove;

        public void OnCreate(ref SystemState state)
        {
            m_QueryInstall = SystemAPI.QueryBuilder()
                .WithAll<Stat, Spawn.Tag>()
                .Build();

            m_QueryUpdate = SystemAPI.QueryBuilder()
                .WithAllRW<StatView>()
                .WithAll<LocalTransform>()
                .WithAspect<StatAspect>()
                .Build();

            m_QueryRemove = SystemAPI.QueryBuilder()
                .WithAll<StatView, DeadTag>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!m_QueryInstall.IsEmpty)
            {
                using var entities = m_QueryInstall.ToEntityArray(Allocator.Temp);
                var writer = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);
                foreach (var entity in entities)
                {
                    var view = new StatView(EnumHandle.FromEnum(Global.Stats.Health));
                    writer.AddComponent(entity, view);
                }
            }

            if (!m_QueryUpdate.IsEmpty)
            {
                state.Dependency = new UpdateJob
                {
                
                }.ScheduleParallel(m_QueryUpdate, state.Dependency);
            }

            if (!m_QueryRemove.IsEmpty)
            {
                state.Dependency = new RemoveJob
                {
                    
                }.ScheduleParallel(m_QueryRemove, state.Dependency);

                var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged);
                ecb.RemoveComponent<StatView>(m_QueryRemove, EntityQueryCaptureMode.AtPlayback);
            }
        }

        partial struct UpdateJob : IJobEntity
        {
            void Execute(ref StatView view, StatAspect stats, in LocalTransform transform)
            {
                view.Update(in stats, in transform);
            }
        }

        partial struct RemoveJob : IJobEntity
        {
            void Execute(StatView view)
            {
                view.Dispose();
            }
        }
    }
}