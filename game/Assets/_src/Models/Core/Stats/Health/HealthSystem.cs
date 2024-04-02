using System;

using Unity.Collections;
using Unity.Entities;
    
namespace Game.Model.Stats
{
    using Logics;

    //[UpdateInGroup(typeof(GameEndSystemGroup))]
    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        private EntityQuery m_Query;
        private Logic.Aspect.Lookup m_LookupLogicAspect;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Stat>()
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Stat>());
            state.RequireForUpdate(m_Query);
            m_LookupLogicAspect = new Logic.Aspect.Lookup(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            m_LookupLogicAspect.Update(ref state);
            state.Dependency = new SystemJob()
            {
                LookupLogicAspect = m_LookupLogicAspect,
            }.ScheduleParallel(m_Query, state.Dependency);
        }

        partial struct SystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public Logic.Aspect.Lookup LookupLogicAspect;

            void Execute(in Entity entity, in DynamicBuffer<Stat> stats)
            {
                var logic = LookupLogicAspect[entity];
                if (logic.IsCurrentAction(Global.Action.Destroy))
                    return;
                if (!stats.TryGetStat(Global.Stats.Health, out Stat health) || (health.Value > 0)) return;
                
                UnityEngine.Debug.Log($"{logic.Self}, {logic.SelfName} [Health system] set Destroy");
                logic.SetEvent(Global.Action.Destroy);
            }
        }
    }
}