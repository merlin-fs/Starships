using System;
using System.Threading;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

namespace Game.Views
{
    using Model.Logics;

    [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
    public partial struct ParticleSystem : ISystem
    {
        private EntityQuery m_Query;
        private WorldTransform m_LookupTransforms;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAspectRO<Logic.Aspect>()
                .WithAll<Particle>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            state.RequireForUpdate(m_Query);
            m_LookupTransforms = state.GetWorldTransformLookup(true);
        }
        
        public void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            state.Dependency = new SystemJob
            {
                LookupTransforms = m_LookupTransforms,
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }

        partial struct SystemJob : IJobEntity
        {
            [ReadOnly] 
            public WorldTransform LookupTransforms;

            public void Execute(in Entity entity, in Logic.Aspect logic, in DynamicBuffer<Particle> particles)
            {
                foreach(var iter in particles)
                {
                    if (logic.IsCurrentAction(iter.Action))
                    {
                        var localEntity = entity;
                        var transform = LookupTransforms.ToWorld(iter.Target);

                        UnityEngine.Debug.Log($"{entity} [Particle] action {logic.CurrentAction}");
                        UnityMainThread.Context.Post(obj =>
                        {
                            ParticleManager.Instance.Play(iter, transform);
                        }, null);
                    }
                }
            }
        }
    }
}
    