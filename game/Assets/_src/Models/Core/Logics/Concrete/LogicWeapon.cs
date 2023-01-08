using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Common.Defs;

namespace Game.Model.Units
{
    using Weapons;
    using Logics;
    using Stats;

    public partial class LogicWeapon : LogicConcreteSystem
    {
        protected override void Init(Logic.LogicDef logic)
        {
            logic.Configure()
                .State(GlobalState.Destroy)

                .Transition(null, null, Weapon.State.Init)

                .Transition(Weapon.State.Init, Weapon.Result.Done, Weapon.State.Shooting)
                .Transition(Weapon.State.Init, Weapon.Result.NoAmmo, Weapon.State.Sleep)

                .Transition(Weapon.State.Shooting, Weapon.Result.Done, Target.State.Find)
                .Transition(Weapon.State.Shooting, Weapon.Result.NoAmmo, Weapon.State.Reload)

                .Transition(Target.State.Find, Target.Result.Found, Weapon.State.Shoot)
                .Transition(Target.State.Find, Target.Result.NoTarget, Weapon.State.Sleep)

                .Transition(Weapon.State.Shoot, Weapon.Result.Done, Weapon.State.Shooting)

                .Transition(Weapon.State.Reload, Weapon.Result.Done, Weapon.State.Shooting)
                .Transition(Weapon.State.Reload, Weapon.Result.NoAmmo, Weapon.State.Sleep)

                .Transition(Weapon.State.Sleep, Weapon.Result.NoAmmo, Weapon.State.Reload)
                .Transition(Weapon.State.Sleep, Target.Result.NoTarget, Weapon.State.Shooting);
        }

        protected override void OnCreate()
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Weapon>()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
                .Build();
            RequireForUpdate(m_Query);
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var ecb = World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer();
            var job = new WeaponJob()
            {
                LogicID = LogicID,
                Teams = GetComponentLookup<Team>(false),
                Writer = ecb.AsParallelWriter(),
                Delta = SystemAPI.Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            Dependency.Complete();
        }

        partial struct WeaponJob : IJobEntity
        {
            public int LogicID;
            public float Delta;
            [ReadOnly]
            public ComponentLookup<Team> Teams;
            public EntityCommandBuffer.ParallelWriter Writer;

            void Execute([EntityIndexInQuery] int idx, ref WeaponAspect weapon, ref LogicAspect logic, 
                ref TransformAspect transform)
            {
                if (!logic.IsSupports(LogicID)) return;

                switch (logic.State)
                {
                    case GlobalState.Destroy:
                        Writer.AddComponent<DeadTag>(idx, logic.Self);
                        break;

                    case Weapon.State.Init:
                        var newClips = (int)weapon.Stat(Weapon.Stats.ClipSize).Value;

                        if (weapon.Reload(new DefExt.WriterContext(Writer, idx), newClips))
                            logic.TrySetResult(Weapon.Result.Done);
                        else
                            logic.TrySetResult(Weapon.Result.NoAmmo);
                        break;

                    case Weapon.State.Reload:
                        weapon.Time += Delta;
                        if (weapon.Time >= weapon.Stat(Weapon.Stats.ReloadTime).Value)
                        {
                            weapon.Time = 0;
                            //TODO: возможно нужно перенести получение кол. патронов...
                            var count = (int)weapon.Stat(Weapon.Stats.ClipSize).Value;

                            if (weapon.Reload(new DefExt.WriterContext(Writer, idx), count))
                                logic.TrySetResult(Weapon.Result.Done);
                            else
                                logic.TrySetResult(Weapon.Result.NoAmmo);
                        }
                        break;


                    case Target.State.Find:
                        if (weapon.Unit != Entity.Null)
                            weapon.SetSoughtTeams(Teams[weapon.Unit].EnemyTeams);
                        else
                            logic.TrySetResult(Target.Result.NoTarget);
                        break;

                    case Weapon.State.Shooting:
                        //TODO: Перенести в отдельный system "Turret"
                        var direction = weapon.Target.WorldTransform.Position;
                        direction = transform.TransformPointWorldToParent(direction) - transform.LocalPosition;
                        transform.LocalRotation = math.nlerp(
                            transform.LocalRotation,
                            quaternion.LookRotationSafe(direction, math.up()),
                            weapon.Time + Delta * 10f);

                        if (weapon.Count == 0)
                        {
                            logic.TrySetResult(Weapon.Result.NoAmmo);
                            return;
                        }

                        weapon.Time += Delta;
                        if (weapon.Time >= weapon.Stat(Weapon.Stats.Rate).Value)
                        {
                            weapon.Time = 0;
                            logic.TrySetResult(Weapon.Result.Done);
                        }
                        break;

                    case Weapon.State.Shoot:
                        weapon.Shot(new DefExt.WriterContext(Writer, idx));
                        logic.TrySetResult(Weapon.Result.Done);
                        break;

                    case Weapon.State.Sleep:
                        weapon.Time += Delta;
                        if (weapon.Time >= weapon.Stat(Weapon.Stats.ReloadTime).Value)
                        {
                            weapon.Time = 0;
                            logic.TrySetResult(logic.Result);
                        }
                        break;
                }
            }
        }
    }
}
