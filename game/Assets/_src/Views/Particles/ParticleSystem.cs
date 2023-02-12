using System;
using Unity.Entities;
using Unity.Transforms;
using Game.Model.Logics;
using System.Threading;

namespace Game.Views
{
    [UpdateInGroup(typeof(GamePresentationSystemGroup))]
    public partial class TestParticleSystem : SystemBase
    {
        private EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .WithAll<Particle>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            new PlayParticleJob
            {
            }
            .ScheduleParallel(m_Query, Dependency)
            .Complete();
        }

        partial struct PlayParticleJob : IJobEntity
        {
            void Execute(in Entity entity, in LogicAspect logic, in DynamicBuffer<Particle> particles, in WorldTransform transform)
            {
                foreach(var iter in particles)
                {
                    if (logic.IsCurrentAction(iter.Action))
                    {
                        var localEntity = entity;
                        var localTransform = transform;
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
    