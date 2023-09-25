using System;
using System.Threading;

using Game.Model.Logics;

using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Core.Animations
{
    public partial struct Animation
    {
        [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
        public partial struct LogicSystem : ISystem
        {
            private EntityQuery m_Query;
            private Logic.Aspect.Lookup m_LookupLogicAspect;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Aspect>()
                    .WithAll<Trigger>()
                    .Build();

                state.RequireForUpdate(m_Query);
                m_LookupLogicAspect = new Logic.Aspect.Lookup(ref state);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupLogicAspect.Update(ref state);
                state.Dependency = new SystemJob
                {
                    LookupLogicAspect = m_LookupLogicAspect,
                }
                .ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Logic.Aspect.Lookup LookupLogicAspect;

                private void Execute(in Entity entity, Aspect animation, in DynamicBuffer<Trigger> triggers)
                {
                    //TODO: нужно все это переделать!
                    bool play = false;
                    Logic.Aspect logic = default;
                    foreach (var iter in triggers)
                    {
                        logic = LookupLogicAspect[iter.LogicEntity];
                        if (logic.IsCurrentAction(iter.Action))
                        {
                            //UnityEngine.Debug.Log($"{entity} [Animation] action: {logic.Action}, clip: {iter.ClipID}");
                            play = true;
                            animation.Play(iter.ClipID, true);
                        }
                    }
                    if (!play && logic.IsAction)
                        animation.Stop();
                }
            }
        }
    }
}