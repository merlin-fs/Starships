using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Units
{
    using Logics;

    public partial class LogicUnit : LogicConcreteSystem
    {
        protected override void Init(Logic.Config logic)
        {
            logic.Configure()
                .Transition(null, null, Move.State.Init)
                .Transition(Move.State.Init, Move.Result.Done, Unit.State.Stop);
        }

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Unit>()
                .WithAll<Logic>()
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic>());
            RequireForUpdate(m_Query);

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var ecb = World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new UnitJob()
            {
                //Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }

        partial struct UnitJob : IJobEntity
        {
            public float Delta;
            //public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, ref UnitAspect unit, ref LogicAspect logic)
            {
                if (logic.Equals(Unit.State.Stop))
                {
                    //logic.SetResult(Result.Done);
                }
            }
        }
    }
}
