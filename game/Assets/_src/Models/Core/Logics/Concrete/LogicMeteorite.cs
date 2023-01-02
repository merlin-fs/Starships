using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Units
{
    using Logics;

    public partial class LogicMeteorite : LogicConcreteSystem
    {
        protected override void Init(Logic.Config logic)
        {
            logic.Configure()
                .Transition(null, null, Move.State.Init)
                .Transition(Move.State.Init, Move.Result.Done, Target.State.Find)

                .Transition(Target.State.Find, Target.Result.Found, Move.State.MoveTo)
                .Transition(Target.State.Find, Target.Result.NoTarget, Target.State.Find)

                .Transition(Move.State.MoveTo, Move.Result.Done, Unit.State.Destroy);
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
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }

        partial struct UnitJob : IJobEntity
        {
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, ref Move data, in UnitAspect unit, in LogicAspect logic, ref Target target)
            {
                if (logic.Equals(Unit.State.Stop))
                {
                    //logic.SetResult(Result.Done);
                }

                if (logic.Equals(Target.State.Find))
                {
                    target.SoughtTeams = unit.Team.EnemyTeams;
                    return;
                }

                if (logic.Equals(Move.State.MoveTo))
                {
                    float3 pos = target.WorldTransform.Position;
                    data.Position = pos;
                    data.Speed = 1f;
                    return;
                }

                if (logic.Equals(Unit.State.Destroy))
                {
                    Writer.DestroyEntity(idx, logic.Self);
                    return;
                }
            }
        }
    }
}
