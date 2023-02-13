using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Game.Model
{
    using Logics;
    using static Logics.Logic;

    public partial struct Move
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct MoveSystem : ISystem
        {
            EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<Move>()
                    .WithAll<Logic>()
                    .Build();

                state.RequireForUpdate(m_Query);
            }

            public void OnDestroy(ref SystemState state) { }

            public void OnUpdate(ref SystemState state)
            {
                var job = new MoveJob()
                {
                    Delta = SystemAPI.Time.DeltaTime,
                };
                state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct MoveJob : IJobEntity
            {
                public float Delta;

                void Execute(ref Move data, ref TransformAspect transform, ref LogicAspect logic)
                {
                    if (logic.IsCurrentAction(Action.Init))
                    {
                        UnityEngine.Debug.Log($"[{logic.Self}] init {data.Position}, speed {data.Speed}");
                        transform.WorldPosition = data.Position;
                        transform.RotateLocal(data.Rotation);
                        logic.SetWorldState(State.Init, true);
                    }

                    if (logic.IsCurrentAction(Action.MoveToTarget) ||
                        logic.IsCurrentAction(Action.MoveToPosition))
                    {
                        //UnityEngine.Debug.Log($"[{logic.Self}] move to target {data.Position}, speed {data.Speed}, pos {transform.WorldPosition}");
                        float3 direction = data.Position - transform.WorldPosition;
                        var dt = math.distancesq(transform.WorldPosition, data.Position);
                        if (dt < 0.1f)
                        {
                            //UnityEngine.Debug.Log($"[{logic.Self}] move done {transform.WorldPosition}, target{data.Position}, dot {dt}");
                            logic.SetWorldState(State.MoveDone, true);
                        }
                        var lookRotation = quaternion.LookRotationSafe(direction, transform.Up);
                        transform.WorldRotation = lookRotation;
                        transform.WorldPosition += math.normalize(direction) * Delta * data.Speed;
                        return;
                    }
                }
            }
        }
    }
}
