using System;
using System.Collections.Generic;

using Common.Core;

using Game.Core.Repositories;

using Unity.Entities;
using Unity.Mathematics;

namespace Game.Core.Animations
{
    public partial struct Animation
    {
        private static class Consts
        {
            private static readonly DIContext.Var<AnimationRepository> m_AnimationRepository;
            public static AnimationRepository Repository => m_AnimationRepository.Value;
        }

        public readonly partial struct Aspect : IAspect
        {
            private readonly Entity m_Self;

            private readonly RefRW<Animation> m_Player;
            private readonly RefRW<CurrentClip> m_CurrentClip;
            private readonly RefRW<NextClip> m_NextClip;

            public bool Playing => m_Player.ValueRO.Playing;

            public void Play(ObjectID clipId, bool loop)
            {
                m_CurrentClip.ValueRW.Data.ClipID = clipId;
                //var config = Consts.Repository.FindByID<EntityAnimatorConfig>("AnimatorDoor");
                //config.GetClip(clipId).
                m_CurrentClip.ValueRW.Data.Elapsed = 0;
                m_CurrentClip.ValueRW.Data.Duration = 1f;//clip.Duration;
                m_CurrentClip.ValueRW.Data.Speed = 1f;//clip.Speed;
                m_CurrentClip.ValueRW.Data.Loop = loop;
                m_Player.ValueRW.Playing = true;
                m_Player.ValueRW.InTransition = false;
            }
            
            public bool GetPosition(int boneId, out float3 value)
            {
                var config = Consts.Repository.FindByID<EntityAnimatorConfig>("AnimatorDoor");
                var clipData = m_CurrentClip.ValueRO.Data;
                var clip = config.GetClip(clipData.ClipID);
                return clip.GetPosition(boneId, clipData.Elapsed, out value);
            }

            public bool GetRotation(int boneId, out quaternion value)
            {
                var config = Consts.Repository.FindByID<EntityAnimatorConfig>("AnimatorDoor");
                var clipData = m_CurrentClip.ValueRO.Data;
                var clip = config.GetClip(clipData.ClipID);
                return clip.GetRotation(boneId, clipData.Elapsed, out value);
            }
        }
    }
}