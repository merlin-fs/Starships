using System;
using Unity.Entities;
using Unity.Collections;

namespace Game.Model.Logics
{
    using Stats;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct FindUnitTarget : Logic.IPartLogic
    {
        public EntityQuery m_Query;
        public ComponentLookup<Team> m_LookupTeam;
        #region IPartLogic
        public EntityQuery Query => m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_LookupTeam = state.GetComponentLookup<Team>(false);
            m_Query = SystemAPI.QueryBuilder()
                .WithAspectRO<Logic.Aspect>()
                .WithAllRW<Target>()
                .WithAll<Team>()
                .Build();
        }

        public void OnDestroy(ref SystemState state) { }
        
        public void OnUpdate(ref SystemState state)
        {
            m_LookupTeam.Update(ref state);
            state.Dependency = new SystemJob()
            {
                Teams = m_LookupTeam,
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
        #endregion
        public partial struct SystemJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<Team> Teams;

            public void Execute(in Logic.Aspect logic, in Team team, ref Target target)
            {
                if (!logic.Def.IsSupportSystem(this))
                    return;

                if (!logic.IsCurrentAction(Target.Action.Find))
                    return;
                
                //UnityEngine.Debug.Log($"{logic.Self} [Logic part] FindUnitTarget set teams");
                target.SoughtTeams = team.EnemyTeams;
                target.Radius = float.MaxValue;
            }
        }
    }
}

/*
namespace Game.Model.Units
{
    using Stats;
    using Logics;
    using Weapons;

    public partial class LogicMeteorite : LogicConcreteSystem
    {
        protected override void Init(Logic.LogicDef logic)
        {
            logic.Configure()
                .Transition(Start, Init)
                .Transition(Init, Find)
                
                .Transition(Find, MoveTo).Condition(Found == true)
                .Transition(Find, MoveTo).Condition(Found == false)

                .Transition(MoveTo, Shot).Condition(Position == Target)

                .Transition(Shot, Stop)

                .Transition(Destroy, Shot).Condition(AOE = true)
                .Transition(Destroy, Stop).Condition(AOE = false)

            //Пересмотреть логику!!!
            logic.Configure()

                .Transition(Logic.State.AnyState, Unit.State.Destroy).Condition(Unit.Params.Destroy)

                .Transition(Logic.State.Start, Move.State.Init).Always()
                .Transition(Move.State.Init, Target.State.Find).Condition(Move.Params.Done)

                .Transition(Target.State.Find, Move.State.MoveTo).Condition(Target.Params.Found)
                    .Else(Move.State.MoveTo)

                .Transition(Move.State.MoveTo, Weapon.State.Shoot).Condition(Move.Params.Done)

                .Transition(Weapon.State.Shoot, Unit.State.Stop).Always()

                .Transition(Unit.State.Destroy, Weapon.State.Shoot).Condition("damage aoe")
                    .Else(Unit.State.Stop);
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
                        if (logic.Result.Equals(Move.Condition.MoveDone))
                        {
                            logic.TrySetResult(Unit.Result.Done);
                            return;
                        }
                        UnityEngine.Debug.Log($"[{logic.Self}] damage count {damages.Length}");
                        var repo = Repositories.Instance.ConfigsAsync().Result;
                        foreach (var iter in damages)
                        {
                            var damageCfg = (DamageConfig)repo.FindByID(iter.DamageConfigID);
                            if (damageCfg.Targets == DamageTargets.AoE)
                            {
                                logic.TrySetResult(Unit.Result.Done);
                                return;
                            }
                        }
                        logic.TrySetResult(Unit.Result.Failed);
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
                        //!!!
                        float3 pos = (logic.Result.Equals(Target.Condition.Dead))
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
                    //!!!
                    logic.TrySetResult(Weapon.Condition.NoAmmo);
                }
            }
        }
    }
}
*/