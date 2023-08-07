using System;
using System.Threading;

using Game.Model.Logics;

using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

namespace Game.Core.Animations
{
    public partial struct Animation
    {
        [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
        public partial struct LogicSystem : ISystem
        {
            private EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Logic.Aspect>()
                    .WithAspect<Aspect>()
                    .WithAll<Trigger>()
                    .Build();

                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                state.Dependency = new SystemJob
                {
                }
                .ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                public void Execute(in Entity entity, Logic.Aspect logic, Aspect animation, in DynamicBuffer<Trigger> triggers)
                {
                    foreach (var iter in triggers)
                    {
                        if (logic.IsCurrentAction(iter.Action))
                        {
                            UnityEngine.Debug.Log($"{entity} [Animation] action {logic.CurrentAction}");
                            animation.Play(iter.ClipID, true);
                        }
                    }
                }
            }
        }
    }
}