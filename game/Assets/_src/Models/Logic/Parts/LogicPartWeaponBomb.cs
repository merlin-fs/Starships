using System;
using Unity.Entities;
using Unity.Collections;

namespace Game.Model.Logics
{
    using Weapons;
    using Stats;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct LogicPartWeaponBomb : Logic.IPartLogic
    {
        EntityQuery m_Query;
        Logic.Aspect.Lookup m_LookupLogicAspect;

        #region IPartLogic
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Root>()
                .WithAll<Logic>()
                .WithAny<LastDamage>()
                .Build();
            m_LookupLogicAspect = new Logic.Aspect.Lookup(ref state, false);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            m_LookupLogicAspect.Update(ref state);
            var system = state.World.GetOrCreateSystemManaged<GameLogicEndCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            state.Dependency = new EmitterMoveJob()
            {
                LookupLogicAspect = m_LookupLogicAspect,
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
        #endregion
        public partial struct EmitterMoveJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public Logic.Aspect.Lookup LookupLogicAspect;

            public void Execute(in Entity self, in DynamicBuffer<LastDamage> damages)
            {
                var logic = LookupLogicAspect[self];

                if (!logic.Def.IsSupportSystem(this))
                    return;

                if (logic.IsCurrentAction(Weapon.Action.Shoot))
                {
                    var logicRoot = LookupLogicAspect[logic.Root];
                    logicRoot.SetEvent(Global.Action.Destroy);
                    logicRoot.SetWorldState(Global.State.Dead, true);

                    logic.SetWorldState(Weapon.State.HasAmo, false);
                    logic.SetWorldState(Global.State.Dead, true);
                    //UnityEngine.Debug.Log($"{logic.Self} [Logic part] Shoot");
                    return;
                }
                
                if (logic.IsCurrentAction(Global.Action.Destroy) && logic.HasWorldState(Global.State.Dead, false))
                {
                    var logicRoot = LookupLogicAspect[logic.Root];
                    logicRoot.SetWorldState(Global.State.Dead, false);

                    logic.SetWorldState(Weapon.State.Active, true);
                    logic.SetWorldState(Global.State.Dead, true);
                    logic.Interrupt();
                }
            }
        }
    }
}