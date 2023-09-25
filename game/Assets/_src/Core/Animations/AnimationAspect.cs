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
        public readonly partial struct Aspect : IAspect
        {
            private static AnimationRepository Repository => Inject<AnimationRepository>.Value;

            private readonly Entity m_Self;

            private readonly RefRW<Animation> m_Player;
            private readonly RefRW<CurrentClip> m_CurrentClip;
            private readonly RefRW<NextClip> m_NextClip;

            public bool Playing => m_Player.ValueRO.Playing;
            public void Play(ObjectID clipId, bool loop)
            {
                if (m_Player.ValueRO.Disable) return;
                
                if (m_CurrentClip.ValueRO.Data.ClipID == clipId && m_Player.ValueRO.Playing) return;
                m_CurrentClip.ValueRW.Data.ClipID = clipId;
                var config = Repository.FindByID<EntityAnimatorConfig>(m_Player.ValueRO.AnimatorID);

                m_CurrentClip.ValueRW.Data.Elapsed = 0;
                m_CurrentClip.ValueRW.Data.Duration = config.GetClip(clipId).Length;
                m_CurrentClip.ValueRW.Data.Speed = 1f;//clip.Speed;
                m_CurrentClip.ValueRW.Data.Loop = config.GetClip(clipId).Loop || loop;
                m_Player.ValueRW.Playing = true;
                m_Player.ValueRW.InTransition = false;
            }

            public void Stop()
            {
                m_Player.ValueRW.Playing = false;
            }
            
            public void SetPosition(int threadId, int boneId, ref float3 value)
            {
                var config = Repository.FindByID<EntityAnimatorConfig>(m_Player.ValueRO.AnimatorID);
                var clipData = m_CurrentClip.ValueRO.Data;
                var clip = config.GetClip(clipData.ClipID);
                clip.SetPosition(threadId, boneId, clipData.Elapsed, ref value);
            }

            public void SetRotation(int threadId, int boneId, ref quaternion value)
            {
                var config = Repository.FindByID<EntityAnimatorConfig>(m_Player.ValueRO.AnimatorID);
                var clipData = m_CurrentClip.ValueRO.Data;
                var clip = config.GetClip(clipData.ClipID);
                clip.SetRotation(threadId, boneId, clipData.Elapsed, ref value);
            }
        }
    }
}