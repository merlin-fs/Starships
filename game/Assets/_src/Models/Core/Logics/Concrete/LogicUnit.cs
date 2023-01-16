using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model.Units
{
    using Game.Model.Stats;

    using Logics;

    public partial class LogicUnit : LogicConcreteSystem
    {
        protected override void Init(Logic.LogicDef logic)
        {
            logic.Configure()
                .State(GlobalState.Destroy)

                .Transition(null, null, Move.State.Init)
                .Transition(Move.State.Init, Move.Result.Done, Unit.State.Stop)
                //.Transition(Move.State.Init, Move.Result.Done, Target.State.Find)
                .Transition(Target.State.Find, Target.Result.NoTarget, Target.State.Find)
                .Transition(Target.State.Find, Target.Result.Found, Target.State.Find);
        }

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Unit>()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
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
                LogicID = LogicID,
                //Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }

        partial struct UnitJob : IJobEntity
        {
            public float Delta;
            public int LogicID;
            //public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, ref UnitAspect unit, ref LogicAspect logic,
                ref TransformAspect transform,
                in Target target)
            {
                if (!logic.IsSupports(LogicID)) return;
            }
        }
    }
}
