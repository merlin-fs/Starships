using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace Game.Views.Stats
{
    using Model.Stats;


    [UpdateInGroup(typeof(GamePresentationSystemGroup))]
    public partial struct CleanupSystem : ISystem
    {
        private EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<StatView>()
                .WithAll<Stat>()
                .WithAll<WorldTransform>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        partial struct UpdateViewJob : IJobEntity
        {
            void Execute(in DynamicBuffer<StatView> views, in StatAspect stats, in TransformAspect transform)
            {
                foreach (var iter in views)
                {
                    var stat = stats.Values.GetRO(iter.StatID);
                    iter.View.Update(stat, transform.WorldTransform);
                }
            }
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new UpdateViewJob
            {
                
            }.ScheduleParallel(m_Query, state.Dependency);
        }
    }
    
    /*
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
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