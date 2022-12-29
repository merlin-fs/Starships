using System;
using Unity.Entities;

namespace Game.Model
{
    using Logics;
    using Result = Logics.Logic.Result;

    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct TargetSystem : ISystem
    {
        EntityQuery m_Query;
               
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Target>()
                .WithAll<Logic>()
                .Build();

            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new Job()
            {
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct Job : IJobEntity
        {
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, ref Target data, ref LogicAspect logic)
            {
                if (logic.Equals(Target.State.Find))
                {
                    //data.Value
                    logic.SetResult(Result.Error);
                }
            }
        }
    }
}
