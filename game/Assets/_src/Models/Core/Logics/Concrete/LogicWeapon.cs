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

    public partial class LogicWeapon : LogicConcreteSystem
    {
        protected override void Init(Logic.Config logic)
        {
            logic.Configure()
                .Transition(null, null, Target.State.Find)

                .Transition(Target.State.Find, Target.Result.Found, Weapon.State.Shooting)

                .Transition(Target.State.Find, Target.Result.NoTarget, Weapon.State.Sleep)

                .Transition(Weapon.State.Shooting, Weapon.Result.Done, Weapon.State.Shoot)
                .Transition(Weapon.State.Shooting, Weapon.Result.NoAmmo, Weapon.State.Reload)

                .Transition(Weapon.State.Shoot, Weapon.Result.Done, Target.State.Find)

                .Transition(Weapon.State.Reload, Weapon.Result.Done, Target.State.Find)
                .Transition(Weapon.State.Reload, Weapon.Result.NoAmmo, Weapon.State.Sleep)

                .Transition(Weapon.State.Sleep, Weapon.Result.NoAmmo, Weapon.State.Reload)
                .Transition(Weapon.State.Sleep, Target.Result.NoTarget, Target.State.Find);
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

            void Execute([EntityIndexInQuery] int entityIndexInQuery, ref WeaponAspect weapon, ref LogicAspect logic, 
                ref TransformAspect transform)
            {
                if (!logic.IsSupports(LogicID)) return;

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
                    //UnityEngine.Debug.Log($"[{logic.Self}] LookAt: {direction}");
                    transform.LookAt(direction);

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
                    weapon.Shot(new DefExt.WriterContext(Writer, entityIndexInQuery));
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
                        if (weapon.Reload(new DefExt.WriterContext(Writer, entityIndexInQuery)))
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
