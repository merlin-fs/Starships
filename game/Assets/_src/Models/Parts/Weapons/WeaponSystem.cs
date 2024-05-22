using System;
using Unity.Entities;
using Game.Model.Logics;
using Reflex.Attributes;
using Reflex.Core;

using Unity.Collections;

namespace Game.Model.Weapons
{
    public partial struct Weapon
    {
        [UpdateInGroup(typeof(GameLogicObjectSystemGroup))]
        public partial class WeaponSystem : SystemBase
        {
            [Inject] private static Container m_Container;
            private static Context.ContextManager<Context> m_ContextManager;
            
            private EntityQuery m_Query;
            [ReadOnly]
            private ComponentLookup<Team> m_LookupTeams;
            private Logic.Aspect.Lookup m_LookupLogicAspect;
            
            protected override void OnCreate()
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<WeaponAspect>()
                    .WithAspect<Logic.Aspect>()
                    .WithAll<Logic.ChangeTag>()
                    .Build();

                m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Logic.ChangeTag>());

                m_LookupLogicAspect = new Logic.Aspect.Lookup(ref CheckedStateRef);
                m_LookupTeams = GetComponentLookup<Team>(true);
                m_ContextManager = new Context.ContextManager<Context>();
                m_ContextManager.Initialization(new Context.ContextGlobal(() => m_Container));
                
                RequireForUpdate(m_Query);
            }

            protected override void OnUpdate()
            {
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                m_LookupLogicAspect.Update(ref CheckedStateRef);
                m_LookupTeams.Update(ref CheckedStateRef);
                
                Dependency = new SystemJob()
                {
                    Delta = SystemAPI.Time.DeltaTime,
                    Writer = system.CreateCommandBuffer(EntityManager.WorldUnmanaged).AsParallelWriter(),
                    LookupLogic = m_LookupLogicAspect,
                    LookupTeams = m_LookupTeams,
                }.ScheduleParallel(m_Query, Dependency);
            }

            partial struct SystemJob : IJobEntity
            {
                public float Delta;
                public EntityCommandBuffer.ParallelWriter Writer;
                [ReadOnly]
                public ComponentLookup<Team> LookupTeams;
                [NativeDisableParallelForRestriction]
                public Logic.Aspect.Lookup LookupLogic;

                void Execute([EntityIndexInQuery] int idx, WeaponAspect weapon)
                {
                    var context = m_ContextManager.Get(new Context.ContextRecord(
                        StructRef.Get(ref weapon), 
                        Delta, 
                        StructRef.Get(ref Writer),
                        idx, 
                        LookupLogic, 
                        LookupTeams));
                    
                    LookupLogic[weapon.Self].ExecuteAction(context);
                    m_ContextManager.Release(context);

                    /* logic
                    if (logic.IsCurrentAction(Global.Action.Init))
                    {
                        UnityEngine.Debug.Log($"{weapon.Self} [Weapon] init");
                        var team = LookupTeams[weapon.Root];
                        var query = new Target.Query {
                            Radius = weapon.Stat(Stats.Range).Value, 
                            SearchTeams = team.EnemyTeams,
                        };
                        Writer.SetComponent(idx, weapon.Self, query);
                    }

                    if (logic.IsCurrentAction(Action.Reload))
                    {
                        weapon.Time += Delta;
                        if (!(weapon.Time >= weapon.Stat(Stats.ReloadTime).Value)) return;
                        
                        weapon.Time = 0;

                        //TODO: нужно перенести получение кол. патронов...
                        //if (!logic.HasWorldState(State.HasAmo, true)) return;
                            
                        var count = (int)weapon.Stat(Stats.ClipSize).Value;
                        logic.SetWorldState(State.HasAmo, weapon.Reload(new WriterContext(Writer, idx), count));
                    }

                    if (logic.IsCurrentAction(Action.Attack))
                    {
                        weapon.Time += Delta;
                        if (!(weapon.Time >= weapon.Stat(Stats.Rate).Value)) return;
                        
                        logic.SetWorldState(State.Shooting, true);
                        weapon.Time = 0;
                        weapon.Shot();
                        
                        if (weapon.Count != 0) return;
                        logic.SetWorldState(State.HasAmo, false);
                        return;
                    }

                    if (logic.IsCurrentAction(Action.Shoot))
                    {
                        logic.SetWorldState(State.Shooting, false);
                        return;
                    }

                    if (logic.IsCurrentAction(Global.Action.Destroy))
                    {
                        logic.SetWorldState(Global.State.Dead, true);
                    }
                    **/
                }
            }
        }
    }
}