using Common.Core;

using Game.Model;
using Game.Model.Logics;
using Game.Model.Weapons;

using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace Game.Core.Animations
{
    public class AnimationTriggerAuthoring : MonoBehaviour
    {
        [SerializeField] private string ClipID;
        
        public class _baker : Baker<AnimationTriggerAuthoring>
        {
            public override void Bake(AnimationTriggerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<Animation.Trigger>(entity);

                buffer.Add(new Animation.Trigger
                {
                    //Action = LogicHandle.FromEnum(Weapon.Action.Shooting),
                    Action = LogicHandle.FromEnum(Target.Action.Find),
                    ClipID = authoring.ClipID,
                });
            }
        }
    }
}