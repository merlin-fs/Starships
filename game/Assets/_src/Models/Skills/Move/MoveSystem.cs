using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    using Logics;
    using Unity.Mathematics;
    using UnityEngine;

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

            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Move>());
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new MoveJob()
            {
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct MoveJob : IJobEntity
        {
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, ref Move data, ref TransformAspect transform, ref LogicAspect logic)
            {
                if (logic.Equals(Move.State.Init))
                {
                    transform.WorldPosition = data.Position;
                    transform.RotateLocal(data.Rotation);
                    logic.SetResult(Move.Result.Done);
                    return;
                }

                if (logic.Equals(Move.State.MoveTo))
                {
                    float3 direction = data.Position - transform.WorldPosition;
                    var dt = math.distancesq(transform.WorldPosition, data.Position);
                    if (dt < 0.1f)
                    {
                        UnityEngine.Debug.Log($"[{logic.Self}] destroy in {transform.WorldPosition}, target{data.Position}, dot {dt}");
                        logic.SetResult(Move.Result.Done);
                    }
                    var lookRotation = quaternion.LookRotation(direction, transform.Up);
                    transform.WorldRotation = lookRotation;
                    transform.WorldPosition += math.normalize(direction) * Delta * data.Speed;
                    return;
                }
            }
        }
    }
}
