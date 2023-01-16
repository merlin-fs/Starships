using System;
using Unity.Entities;
using Unity.Transforms;
using Game.Model.Logics;
using Unity.Collections;

namespace Game.Views
{
    [UpdateInGroup(typeof(GamePresentationSystemGroup))]
    public partial class TestParticleSystem : SystemBase
    {
        private EntityQuery m_Query;
        private ComponentLookup<WorldTransform> m_LookupWorldTransform;

        protected override void OnCreate()
        {
            m_LookupWorldTransform = GetComponentLookup<WorldTransform>();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .WithAll<Particle>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            m_LookupWorldTransform.Update(ref CheckedStateRef);

            new PlayParticleJob
            {
                LookupWorldTransform = m_LookupWorldTransform,
            }
            .ScheduleParallel(m_Query, Dependency)
            .Complete();
        }

        partial struct PlayParticleJob : IJobEntity
        {
            [ReadOnly, NativeDisableParallelForRestriction]
            public ComponentLookup<WorldTransform> LookupWorldTransform;

            void Execute(in Entity entity, in LogicAspect logic, in DynamicBuffer<Particle> particles)
            {
                foreach(var iter in particles)
                {
                    if (iter.StateID == logic.StateID)
                    {
                        ParticleManager.Play(entity, iter.StateID, LookupWorldTransform[iter.Target]);
                    }
                }
            }
        }

        //void RecursiveChilds(Entity entity, BufferLookup<Child> childs, Action<Entity> action)
        //{
        //    action?.Invoke(entity);
        //    if (!childs.HasBuffer(entity))
        //        return;
        //    var child = childs[entity];
        //    for (var i = 0; i < child.Length; ++i)
        //    {
        //        var iter = child[i].Value;
        //        RecursiveChilds(iter, childs, action);
        //    }
        //}
    }
    }
    