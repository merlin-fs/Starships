using Game.Model;
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
                    //Action = EnumHandle.FromEnum(Target.Action.Find),
                    Action = EnumHandle.FromEnum(Move.Action.Init),
                    ClipID = authoring.ClipID,
                });
            }
        }
    }
}