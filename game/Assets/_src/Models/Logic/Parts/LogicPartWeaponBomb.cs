using System;
using Unity.Entities;

namespace Game.Model.Logics
{
    using Weapons;
    using Stats;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct LogicPartWeaponBomb : Logic.IPartLogic
    {
        EntityQuery m_Query;

        #region IPartLogic
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Root>()
                .WithAll<Logic>()
                .WithAny<LastDamage>()
                .WithNone<DeadTag>()
                .Build();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new EmitterMoveJob()
            {
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
        #endregion
        public partial struct EmitterMoveJob : IJobEntity
        {
            public void Execute(in Entity self, ref LogicAspect logic)
            {
                if (!logic.Def.IsSupportSystem(this))
                    return;

                if (logic.IsCurrentAction(Weapon.Action.Shoot))
                {
                    logic.SetWorldState(Weapon.State.HasAmo, false);
                }
            }
        }
    }
}