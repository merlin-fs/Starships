using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    using Logics;
    using Result = Logics.Logic.Result;

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
                    logic.SetResult(Result.Done);
                }
            }
        }
    }
}
