using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Common.Defs;

namespace Game.Model.Units
{
    using Stats;
    using Logics;
    using Weapons;
    using Game.Core.Repositories;
    using UnityEngine.XR;

    public partial class LogicMeteorite : LogicConcreteSystem
    {
        protected override void Init(Logic.LogicDef logic)
        {
            logic.Configure()
                .State(GlobalState.Destroy)

                .Transition(null, null, Move.State.Init)
                .Transition(Move.State.Init, Move.Result.Done, Target.State.Find)

                .Transition(Target.State.Find, Target.Result.Found, Move.State.MoveTo)
                .Transition(Target.State.Find, Target.Result.NoTarget, Move.State.MoveTo)

                .Transition(Move.State.MoveTo, Move.Result.Done, Weapon.State.Shoot)
                .Transition(Weapon.State.Shoot, Weapon.Result.Done, Unit.State.Stop)
                .Transition(GlobalState.Destroy, Unit.Result.Done, Unit.State.Stop);
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
                ref WeaponAspect weapon, in DynamicBuffer<LastDamages> damages,
                ref Move data)
            {
                if (!logic.IsSupports(LogicID)) return;

                switch (logic.State)
                {
                    case Unit.State.Stop:
                        Writer.AddComponent<DeadTag>(idx, logic.Self);
                        break;

                    case GlobalState.Destroy:

                        var repo = Repositories.Instance.ConfigsAsync().Result;
                        foreach (var iter in damages)
                        {
                            var damageCfg = (DamageConfig)repo.FindByID(iter.DamageConfigID);
                            if (damageCfg.Targets == DamageTargets.AoE)
                            {
                                UnityEngine.Debug.Log($"[{logic.Self}], self explose");
                                Shot(ref weapon, ref logic, Writer);
                                break;
                            }
                        }
                        logic.TrySetResult(Unit.Result.Done);
                        break;

                    case Weapon.State.Shoot:
                        Shot(ref weapon, ref logic, Writer);
                        break;

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
                }

                void Shot(ref WeaponAspect weapon, ref LogicAspect logic, EntityCommandBuffer.ParallelWriter Writer)
                {
                    weapon.Target = new Target { Value = weapon.Self };
                    weapon.Shot(new DefExt.WriterContext(Writer, idx));
                    logic.TrySetResult(Weapon.Result.Done);
                }
            }
        }
    }
}
