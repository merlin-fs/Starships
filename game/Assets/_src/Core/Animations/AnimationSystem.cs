using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Game.Core.Animations
{
    public partial struct Animation
    {
        [UpdateInGroup(typeof(GameLogicEndSystemGroup))]
        public partial struct System : ISystem
        {
            private EntityQuery m_QueryBones;
            private EntityQuery m_QueryAnimator;
            private Aspect.Lookup m_LookupAspect;

            public void OnCreate(ref SystemState state)
            {
                m_QueryBones = SystemAPI.QueryBuilder()
                    .WithAll<Bone, LocalTransform>()
                    .Build();

                m_QueryAnimator = SystemAPI.QueryBuilder()
                    .WithAll<Animation, CurrentClip, NextClip>()
                    .Build();

                m_LookupAspect = new Aspect.Lookup(ref state);
            }

            public void OnUpdate(ref SystemState state)
            {
                m_LookupAspect.Update(ref state);
                state.Dependency = new SystemJob
                {
                    LookupAspect = m_LookupAspect
                }
                .ScheduleParallel(m_QueryBones, state.Dependency);

                state.Dependency = new UpdateAnimatorJob
                {
                    DT = SystemAPI.Time.DeltaTime,
                }.ScheduleParallel(m_QueryAnimator, state.Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                [NativeSetThreadIndex]
                int m_ThreadIndex;
                [NativeDisableParallelForRestriction]
                public Aspect.Lookup LookupAspect;

                private void Execute(in Bone bone, ref LocalTransform localTransform)
                {
                    var animator = LookupAspect[bone.Animator];
                    if (!animator.Playing) return;

                    animator.SetPosition(m_ThreadIndex, bone.BoneID, ref localTransform.Position);
                    animator.SetRotation(m_ThreadIndex, bone.BoneID, ref localTransform.Rotation);
                }
            }

            [BurstCompile]
            partial struct UpdateAnimatorJob : IJobEntity
            {
                public float DT;

                private void Execute(ref Animation animation, ref CurrentClip currentClip, ref NextClip nextClip)
                {
                    if (!animation.Playing) return;

                    // Update elapsed time
                    currentClip.Data.Elapsed += DT * currentClip.Data.Speed * animation.SpeedMultiplier;
                    nextClip.Data.Elapsed += DT * nextClip.Data.Speed * animation.SpeedMultiplier;

                    if (currentClip.Data.Loop)
                    {
                        currentClip.Data.Elapsed %= currentClip.Data.Duration;
                    }
                    else
                    {
                        currentClip.Data.Elapsed = math.min(currentClip.Data.Elapsed, currentClip.Data.Duration);
                    }

                    if (nextClip.Data.Loop)
                    {
                        nextClip.Data.Elapsed %= nextClip.Data.Duration;
                    }
                    else
                    {
                        nextClip.Data.Elapsed = math.min(nextClip.Data.Elapsed, nextClip.Data.Duration);
                    }

                    // Update transition
                    if (animation.InTransition)
                    {
                        animation.TransitionElapsed += DT;
                        if (animation.TransitionElapsed >= animation.TransitionDuration)
                        {
                            animation.InTransition = false;
                            animation.TransitionElapsed = 0;
                            animation.TransitionDuration = 0;
                            currentClip.Data.ClipID = nextClip.Data.ClipID;
                            currentClip.Data.Duration = nextClip.Data.Duration;
                            currentClip.Data.Elapsed = nextClip.Data.Elapsed;
                            currentClip.Data.Speed = nextClip.Data.Speed;
                            currentClip.Data.Loop = nextClip.Data.Loop;
                        }
                    }
                }
            }
        }
    }
}