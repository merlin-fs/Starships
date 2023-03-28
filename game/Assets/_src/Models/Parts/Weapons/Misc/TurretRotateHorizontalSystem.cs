using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Common.Defs;

namespace Game.Model.Weapons
{
    using Logics;

    using Stats;

    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct TurretRotateHorizontalSystem : ISystem
    {
        EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithAll<Logic>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var job = new SystemJob()
            {
                Delta = SystemAPI.Time.DeltaTime,
            };
            //state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency = job.Schedule(m_Query, state.Dependency);
        }

        partial struct SystemJob : IJobEntity
        {
            public float Delta;
            public void Execute(ref WeaponAspect weapon, in Logic.Aspect logic)
            {
                if (logic.IsCurrentAction(Weapon.Action.Shooting) && weapon.Target.Value != Entity.Null)
                {
                    var transform = weapon.Transform;
                    var direction = weapon.Target.Transform.Position;
                    
                    //direction = transform.TransformPointWorldToParent(direction) - transform.LocalPosition;
                    direction = direction - transform.Position;

                    direction.y = transform.Position.y;
                    
                    transform.Rotation = math.nlerp(
                        weapon.Transform.Rotation,
                        quaternion.LookRotationSafe(direction, math.up()),
                        weapon.Time + Delta * 10f);
                }
            }
        }
    }
} 