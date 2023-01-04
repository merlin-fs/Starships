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
                .Transition(Target.State.Find, Target.Result.NoTarget, Target.State.Find)

                .Transition(Move.State.MoveTo, Move.Result.Done, Weapon.State.Shoot);
        }

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Unit>()
                .WithAll<Weapon>()
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

            void Execute([EntityIndexInQuery] int idx, in UnitAspect unit, in LogicAspect logic,
                ref WeaponAspect weapon,
                ref Move data)
            {
                if (!logic.IsSupports(LogicID)) return;

                if (logic.Equals(Unit.State.Stop))
                {
                    //logic.SetResult(Result.Done);
                }

                if (logic.Equals(Target.State.Find))
                {
                    weapon.SetSoughtTeams(unit.Team.EnemyTeams);
                    return;
                }

                if (logic.Equals(Move.State.MoveTo))
                {
                    float3 pos = weapon.Target.WorldTransform.Position;
                    data.Position = pos;
                    data.Speed = unit.Stat(Unit.Stats.Speed).Value;
                    return;
                }

                if (logic.Equals(Weapon.State.Shoot))
                {
                    //weapon.Shot(new DefExt.WriterContext(Writer, idx));
                    //Writer.AddComponent<DeadTag>(idx, logic.Self);
                    //Writer.DestroyEntity(idx, logic.Self);
                    return;
                }
            }
        }
    }
}
