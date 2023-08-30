using System;

using Game.Model;
using Game.Model.Weapons;

using UnityEngine;
using Unity.Entities;

namespace Game.Core.Animations
{
    public class AnimationTriggerAuthoring : MonoBehaviour
    {
        [SelectChildPrefab]
        public GameObject Logic;

        [SerializeField] private string ClipID;
        
        public class _baker : Baker<AnimationTriggerAuthoring>
        {
            public override void Bake(AnimationTriggerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<Animation.Trigger>(entity);
                var root = entity;
                if (authoring.Logic)
                    root = GetEntity(authoring.Logic, TransformUsageFlags.Dynamic);

                buffer.Add(new Animation.Trigger
                {
                    LogicEntity = root,
                    Action = EnumHandle.FromEnum(Target.Action.Find),
                    //Action = EnumHandle.FromEnum(Move.Action.Init),
                    ClipID = authoring.ClipID,
                });
                buffer.Add(new Animation.Trigger
                {
                    LogicEntity = root,
                    Action = EnumHandle.FromEnum(Move.Action.FindPath),
                    //Action = EnumHandle.FromEnum(Move.Action.Init),
                    ClipID = authoring.ClipID,
                });
                buffer.Add(new Animation.Trigger
                {
                    LogicEntity = root,
                    Action = EnumHandle.FromEnum(Weapon.Action.Attack),
                    //Action = EnumHandle.FromEnum(Move.Action.Init),
                    ClipID = authoring.ClipID,
                });
                buffer.Add(new Animation.Trigger
                {
                    LogicEntity = root,
                    Action = EnumHandle.FromEnum(Weapon.Action.Shoot),
                    //Action = EnumHandle.FromEnum(Move.Action.Init),
                    ClipID = authoring.ClipID,
                });
            }
        }
    }
}