using System;
using Common.Core;

using Game.Model.Logics;

using Unity.Entities;

namespace Game.Core.Animations
{
    public partial struct Animation : IComponentData
    {
        public ObjectID AnimatorID;
        public bool Playing;
        public bool Disable;
        public bool InTransition;
        
        public float TransitionDuration;
        public float TransitionElapsed;

        public float SpeedMultiplier;
    }

    public partial struct Animation
    {
        public static int ArrayLength;

        public static void Init()
        {
            ArrayLength = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount + 2;
        }

        public struct Trigger : IBufferElementData
        {
            public Entity LogicEntity;
            public EnumHandle Action;
            public ObjectID ClipID;

            public bool Scale;
            public bool Position;
            public bool Rotation;
            public float ScaleTime;
        }
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