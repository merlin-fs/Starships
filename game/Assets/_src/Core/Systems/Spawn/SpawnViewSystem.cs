using System;

using Game.Core.Prefabs;
using Game.Core.Repositories;

using Reflex.Core;
using Reflex.Attributes;

using Unity.Entities;

namespace Game.Core.Spawns
{
    public partial struct Spawn
    {
        [UpdateInGroup(typeof(GameSpawnSystemGroup))]
        [UpdateBefore(typeof(CleanupSystem))]
        public partial struct SpawnViewSystem : ISystem
        {
            [Inject] private static ObjectRepository m_Repository;
            [Inject] private static Container m_Container;
            private EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<ViewTag, Tag, PrefabInfo.BakedInnerPathPrefab>()
                    .Build();
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
                var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

                foreach (var (info, children, entity) in SystemAPI.Query<PrefabInfo, DynamicBuffer<PrefabInfo.BakedInnerPathPrefab>>().WithAll<Tag, ViewTag>().WithEntityAccess())
                {
                    var config = m_Repository.FindByID(info.ConfigID);
                    if (config == null) throw new ArgumentNullException($"Prefab {info.ConfigID} not found");

                    var builder = Spawner.SpawnView(config, entity, children, ecb, m_Container);
                }
                ecb.RemoveComponent<ViewTag>(m_Query, EntityQueryCaptureMode.AtPlayback);
            }
        }
    }
}