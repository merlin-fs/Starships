using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Game.Model
{
    using Game.Model.Stats;

    using Logics;

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct MoveSystem : ISystem
    {
        EntityQuery m_Query;
               
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Move>()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Move>());
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

            void Execute(in Move data, ref TransformAspect transform, ref LogicAspect logic)
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
                        logic.SetResult(Move.Result.Done);

                    var lookRotation = quaternion.LookRotation(direction, transform.Up);
                    transform.WorldRotation = lookRotation;
                    transform.WorldPosition += math.normalize(direction) * Delta * data.Speed;
                    return;
                }
            }
        }
    }
}
