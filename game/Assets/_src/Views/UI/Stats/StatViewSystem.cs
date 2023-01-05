using System;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace Game.Views.Stats
{
    using Game.Model;
    using Model.Stats;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial class HealthAddSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityCommandBufferSystem m_CommandBuffer;

        public Canvas CanvasParent;
        //public HealthComponent Prefab;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .WithAll<Spawn>()
                .Build();
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            var entities = m_Query.ToEntityArray(Allocator.Temp);
            var writer = m_CommandBuffer.CreateCommandBuffer();
            foreach (var entity in entities)
            {
                //var view = GameObject.Instantiate<HealthComponent>(Prefab, CanvasParent.transform);
                //view.Value.SetPosition(pos);
                //writer.AddComponent<HealthView>(entity, new HealthView(view));
            }
            entities.Dispose();
        }
    }
    /*
    [DisableAutoCreation]
    [UpdateInGroup(typeof(GameDoneSystemGroup))]
    public partial class HealthDelSystem : SystemBase
    {
        private EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<HealthView>(),
                ComponentType.ReadOnly<StateDead>()
            );
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            var views = m_Query.ToComponentDataArray<HealthView>(Allocator.Temp);
            foreach (var view in views)
            {
                view.Value.SetDestroy();
                view.Dispose();
            }
            views.Dispose();
        }
    }
    */
}