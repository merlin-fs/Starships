using System;
using Common.Core;
using Unity.Entities;

namespace Game.Core.Animations
{
    public partial struct Animation : IComponentData
    {
        public bool Playing;
        public bool InTransition;
        
        public float TransitionDuration;
        public float TransitionElapsed;

        public float SpeedMultiplier;
    }

    public partial struct Animation
    {
        public struct Bone : IComponentData
        {
            public Entity Animator;
            public int BoneID;
        }

        public struct ClipData
        {
            public ObjectID ClipID;
            public float Duration;
            public float Elapsed;
            public float Speed;
            public bool Loop;
        }

        public struct CurrentClip : IComponentData
        {
            public ClipData Data;
        }
    
        public struct NextClip : IComponentData
        {
            public ClipData Data;
        }
    }
}