using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Common.Defs;

namespace Game.Model.Units
{
    using Weapons;
    using Logics;
    using Game.Model.Stats;
    using TMPro;
    using Unity.Mathematics;
    using static Game.Model.Logics.Logic;

    public partial class LogicWeapon : LogicConcreteSystem
    {
        protected override void Init(Logic.Config logic)
        {
            logic.Configure()
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

                if (logic.Equals(Weapon.State.Init))
                {
                    var count = (int)weapon.Stat(Weapon.Stats.ClipSize).Value;

                    if (weapon.Reload(new DefExt.WriterContext(Writer, idx), count))
                        logic.SetResult(Weapon.Result.Done);
                    else
                        logic.SetResult(Weapon.Result.NoAmmo);
                    return;
                }

                if (logic.Equals(Target.State.Find))
                {
                    if (weapon.Unit != Entity.Null)
                        weapon.SetSoughtTeams(Teams[weapon.Unit].EnemyTeams);
                    else
                        logic.SetResult(Target.Result.NoTarget);

                    return;
                }

                if (logic.Equals(Weapon.State.Shooting))
                {
                    var direction = weapon.Target.WorldTransform.Position;

                    direction = transform.TransformPointWorldToParent(direction) - transform.LocalPosition;
                    transform.LocalRotation = math.nlerp(
                        transform.LocalRotation, 
                        quaternion.LookRotationSafe(direction, math.up()),
                        weapon.Time + Delta * 10f);

                    if (weapon.Count == 0)
                    {
                        logic.SetResult(Weapon.Result.NoAmmo);
                        return;
                    }
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Stat(Weapon.Stats.Rate).Value)
                    {
                        weapon.Time = 0;
                        logic.SetResult(Weapon.Result.Done);
                    }
                    return;
                }

                if (logic.Equals(Weapon.State.Shoot))
                {
                    weapon.Shot(new DefExt.WriterContext(Writer, idx));
                    logic.SetResult(Weapon.Result.Done);
                    return;
                }

                if (logic.Equals(Weapon.State.Sleep))
                {
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Stat(Weapon.Stats.ReloadTime).Value)
                    {
                        weapon.Time = 0;
                        logic.SetResult(logic.Result);
                    }
                    return;
                }

                if (logic.Equals(Weapon.State.Reload))
                {
                    weapon.Time += Delta;
                    if (weapon.Time >= weapon.Stat(Weapon.Stats.ReloadTime).Value)
                    {
                        weapon.Time = 0;
                        //TODO: возможно нужно перенести получение кол. патронов...
                        var count = (int)weapon.Stat(Weapon.Stats.ClipSize).Value;

                        if (weapon.Reload(new DefExt.WriterContext(Writer, idx), count))
                            logic.SetResult(Weapon.Result.Done);
                        else
                            logic.SetResult(Weapon.Result.NoAmmo);
                    }
                    return;
                }
            }
        }
    }
}
