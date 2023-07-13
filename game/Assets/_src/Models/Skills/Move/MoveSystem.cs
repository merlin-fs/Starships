using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Model
{
    using Logics;
    
    public partial struct Move
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct MoveSystem : ISystem
        {
            EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAllRW<Move>()
                    .WithAllRW<LocalTransform>()
                    .WithAspect<Logic.Aspect>()
                    .Build();

                state.RequireForUpdate(m_Query);
            }

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

                public void Execute(ref Move data, ref LocalTransform transform, Logic.Aspect logic)
                {
                    if (logic.IsCurrentAction(Action.Init))
                    {
                        UnityEngine.Debug.Log($"{logic.Self} [Move] init {data.Position}, speed {data.Speed}");
                        transform.Position = data.Position;
                        transform = transform.Rotate(data.Rotation);
                        logic.SetWorldState(State.Init, true);
                    }

                    if (logic.IsCurrentAction(Action.MoveToTarget) || logic.IsCurrentAction(Action.MoveToPosition))
                    {
                        //UnityEngine.Debug.Log($"[{logic.Self}] move to target {data.Position}, speed {data.Speed}, pos {transform.WorldPosition}");
                        float3 direction = data.Position - transform.Position;
                        var dt = math.distancesq(transform.Position, data.Position);
                        if (dt < 0.1f)
                        {
                            //UnityEngine.Debug.Log($"[{logic.Self}] move done {transform.WorldPosition}, target{data.Position}, dot {dt}");
                            logic.SetWorldState(State.MoveDone, true);
                        }
                        
                        var lookRotation = quaternion.LookRotationSafe(direction, math.up());
                        transform.Rotation = lookRotation;
                        transform.Position += math.normalize(direction) * Delta * data.Speed;
                        return;
                    }
                }
            }
        }
    }
}
