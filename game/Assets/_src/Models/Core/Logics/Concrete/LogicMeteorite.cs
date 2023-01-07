using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;

namespace Game.Model.Units
{
    using Stats;
    using Logics;
    using Weapons;
    
    public partial class LogicMeteorite : LogicConcreteSystem
    {
        protected override void Init(Logic.Config logic)
        {
            logic.Configure()
                .Transition(null, null, Move.State.Init)
                .Transition(Move.State.Init, Move.Result.Done, Target.State.Find)

                .Transition(Target.State.Find, Target.Result.Found, Move.State.MoveTo)
                .Transition(Target.State.Find, Target.Result.NoTarget, Move.State.MoveTo)

                .Transition(Move.State.MoveTo, Move.Result.Done, Weapon.State.Shoot)
                .Transition(Weapon.State.Shoot, Weapon.Result.Done, Unit.State.Destroy);
        }

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Unit>()
                .WithAll<Weapon>()
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
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }

        partial struct UnitJob : IJobEntity
        {
            public int LogicID;
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, in UnitAspect unit, ref LogicAspect logic,
                ref WeaponAspect weapon,
                ref Move data)
            {
                if (!logic.IsSupports(LogicID)) return;

                switch (logic.State)
                {
                    case Move.State.Init:
                        weapon.Reload(new DefExt.WriterContext(Writer, idx), 0);
                        break;

                    case Target.State.Find:
                        weapon.SetSoughtTeams(unit.Team.EnemyTeams);
                        break;

                    case Move.State.MoveTo:
                        float3 pos = (logic.Result.Equals(Target.Result.NoTarget))
                            ? float3.zero
                            : weapon.Target.WorldTransform.Position;

                        data.Position = pos;
                        data.Speed = unit.Stat(Unit.Stats.Speed).Value;
                        break;

                    case Weapon.State.Shoot:
                        weapon.Target = new Target { Value = weapon.Self };
                        logic.SetResult(Weapon.Result.Done);
                        break;

                    case Unit.State.Destroy:
                        Writer.AddComponent<DeadTag>(idx, logic.Self);
                        break;

                }
            }
        }
    }
}
