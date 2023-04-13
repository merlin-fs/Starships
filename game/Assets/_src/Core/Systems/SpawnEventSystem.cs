using System;
using Unity.Entities;

namespace Game.Systems
{
    using Buildings;
    using Model;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct SpawnEventSystem : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SpawnEventTag>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        partial struct SystemJob : IJobEntity
        {
            private BuildingContext.Var<IApiEditorHandler> m_ApiHandler;
            void Execute(in Entity entity)
            {
                m_ApiHandler.Value.OnSpawn(entity);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
            state.Dependency = new SystemJob()
            {
            }.ScheduleParallel(m_Query, state.Dependency);
            ecb.RemoveComponent<SpawnEventTag>(m_Query);
        }
    }
}