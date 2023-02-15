using System;
using System.Threading;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

namespace Game.Views
{
    using Model.Logics;

    [UpdateInGroup(typeof(GamePresentationSystemGroup))]
    public partial struct ParticleSystem : ISystem
    {
        private EntityQuery m_Query;
        ComponentLookup<WorldTransform> m_LookupTransforms;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .WithAll<Particle>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            state.RequireForUpdate(m_Query);
            m_LookupTransforms = state.GetComponentLookup<WorldTransform>(true);
        }
        
        public void OnDestroy(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            state.Dependency = new PlayParticleJob
            {
                LookupTransforms = m_LookupTransforms,
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }

        partial struct PlayParticleJob : IJobEntity
        {
            [ReadOnly] 
            public ComponentLookup<WorldTransform> LookupTransforms;

            void Execute(in Entity entity, in LogicAspect logic, in DynamicBuffer<Particle> particles)
            {
                foreach(var iter in particles)
                {
                    if (logic.IsCurrentAction(iter.Action))
                    {
                        var localEntity = entity;
                        var localTransform = LookupTransforms[iter.Target];
                        UnityEngine.Debug.Log($"{entity} [Particle] action {logic.CurrentAction}");
                        UnityMainThread.Context.Post(obj =>
                        {
                            ParticleManager.Instance.Play(localEntity, iter, localTransform);
                        }, null);
                    }
                }
            }
        }
    }
}
    