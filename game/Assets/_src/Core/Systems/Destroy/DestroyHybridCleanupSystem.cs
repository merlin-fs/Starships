using Game.Core.Prefabs;
using Unity.Entities;
using UnityEngine;

namespace Game.Systems
{
    using Model.Stats;

    [UpdateInGroup(typeof(GameSpawnSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(DestroyCleanupSystem))]
    public partial struct DestroyHybridCleanupSystem : ISystem
    {
        EntityQuery m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<DeadTag, PrefabInfo.ContextReference>()
                .Build();

            state.RequireForUpdate(m_Query);
        }
        public void OnUpdate(ref SystemState state)
        {
            var system = state.World.GetOrCreateSystemManaged<GameLogicEndCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            foreach (var context in SystemAPI. Query<PrefabInfo.ContextReference>().WithAll<DeadTag>())
            {
                var view = context.Value.Resolve<GameObject>();
                Object.Destroy(view, 0.1f);
            }
            ecb.RemoveComponent<PrefabInfo.ContextReference>(m_Query, EntityQueryCaptureMode.AtRecord);
        }
    }
}