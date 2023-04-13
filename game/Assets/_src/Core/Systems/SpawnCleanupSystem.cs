using System;
using Unity.Entities;

namespace Game.Systems
{
    using Model;

    [UpdateInGroup(typeof(GameSpawnSystemGroup), OrderLast = true)]
    partial struct SpawnCleanupSystem : ISystem
    {
        EntityQuery m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SpawnTag>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnUpdate(ref SystemState state)
        {
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
            ecb.RemoveComponent<SpawnTag>(m_Query);
        }
    }
}