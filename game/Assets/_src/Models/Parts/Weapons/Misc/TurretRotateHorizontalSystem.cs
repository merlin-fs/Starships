﻿using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Model.Weapons
{
    using Logics;

    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct TurretRotateHorizontalSystem : ISystem
    {
        EntityQuery m_Query;
        WorldTransform m_WorldTransform;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAspect<WeaponAspect>()
                .WithAspectRO<Logic.Aspect>()
                .Build();
            state.RequireForUpdate(m_Query);
            m_WorldTransform = state.GetWorldTransformLookup(false);
            
        }

        public void OnUpdate(ref SystemState state)
        {
            m_WorldTransform.Update(ref state);
            var job = new SystemJob()
            {
                Delta = SystemAPI.Time.DeltaTime,
                WorldTransform = m_WorldTransform,
            };
            state.Dependency = job.Schedule(m_Query, state.Dependency);
        }

        partial struct SystemJob : IJobEntity
        {
            public float Delta;
            public WorldTransform WorldTransform;
            public void Execute(ref WeaponAspect weapon, in Logic.Aspect logic, in Parent parent)
            {
                if (logic.IsCurrentAction(Weapon.Action.Shooting) && weapon.Target.Value != Entity.Null)
                {
                    var transform = WorldTransform.GetToWorldRefRW(weapon.Self).ValueRO;
                    var direction = weapon.Target.Transform.Position;
                    //var direction = WorldTransform.ToWorld(weapon.Target.Value)
                    //direction = transform.TransformPointWorldToParent(direction) - transform.LocalPosition;
                    direction = direction - transform.Position;
                    direction.y = transform.Position.y;

                    ref var local = ref WorldTransform.GetTransformRefRW(weapon.Self).ValueRW;
                    local.Rotation = math.nlerp(
                        transform.Rotation,
                        quaternion.LookRotationSafe(direction, math.up()),
                        weapon.Time + Delta * 10f);
                }
            }
        }
    }
} 