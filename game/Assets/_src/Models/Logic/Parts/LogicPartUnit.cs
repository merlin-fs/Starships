using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Logics
{
    using Units;
    using Stats;
    using Game.Model.Weapons;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct LogicPartUnit : Logic.IPartLogic
    {
        public EntityQuery m_Query;
        BufferLookup<Child> m_LookupChildren;
        Logic.Aspect.Lookup m_LookupLogicAspect;
        ComponentLookup<Logic> m_LookupLogic;

        #region IPartLogic
        public EntityQuery Query => m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .WithAll<Unit>()
                .Build();

            m_LookupLogic = state.GetComponentLookup<Logic>(true);
            m_LookupLogicAspect = new Logic.Aspect.Lookup(ref state, false);
            m_LookupChildren = state.GetBufferLookup<Child>(true);
        }

        public void OnDestroy(ref SystemState state) { }
        
        public void OnUpdate(ref SystemState state)
        {
            m_LookupLogic.Update(ref state);
            m_LookupLogicAspect.Update(ref state);
            m_LookupChildren.Update(ref state);
            state.Dependency = new SystemJob()
            {
                LookupLogic = m_LookupLogic,
                LookupLogicAspect = m_LookupLogicAspect,
                LookupChildren = m_LookupChildren,
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
        #endregion
        public partial struct SystemJob : IJobEntity
        {
            [ReadOnly]
            public BufferLookup<Child> LookupChildren;
            [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public Logic.Aspect.Lookup LookupLogicAspect;
            [ReadOnly, NativeDisableContainerSafetyRestriction]
            public ComponentLookup<Logic> LookupLogic;
            public void Execute(Entity self, ref Logic.Aspect logic)
            {
                if (!logic.Def.IsSupportSystem(this))
                    return;

                if (logic.IsCurrentAction(Unit.Action.ActiveWeapons))
                {
                    var lookupLogicAspect = LookupLogicAspect;
                    var lookupLogic = LookupLogic;
                    LookupChildren.ChildrenForEach(self, iter =>
                    {
                        if (lookupLogic.HasComponent(iter))
                        {
                            lookupLogicAspect[iter].SetWorldState(Weapon.State.Active, true);
                        }
                    });
                    logic.SetWorldState(Unit.State.WeaponsActive, true);
                    return;
                }

                if (logic.IsCurrentAction(Global.Action.Destroy))
                {
                    logic.SetWorldState(Global.State.Dead, true);
                }
            }
        }
    }
}