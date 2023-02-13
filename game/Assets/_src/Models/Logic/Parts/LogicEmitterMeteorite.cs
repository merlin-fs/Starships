using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game.Model.Logics
{
    using Weapons;
    using Stats;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct LogicEmitterMeteorite : Logic.IPartLogic
    {
        EntityQuery m_Query;
        BufferLookup<Logic.WorldState> m_LookupWorldStates;

        #region IPartLogic
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Root>()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
                .Build();
            m_LookupWorldStates = state.GetBufferLookup<Logic.WorldState>(true);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            m_LookupWorldStates.Update(ref state);
            var ecb = state.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
            state.Dependency = new EmitterMoveJob()
            {
                Writer = ecb.AsParallelWriter(),
                LookupWorldStates = m_LookupWorldStates,
            }
            .ScheduleParallel(m_Query, state.Dependency);

            //state.World.
            //state.SystemHandle
        }
        #endregion
        public partial struct EmitterMoveJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            [ReadOnly, NativeDisableContainerSafetyRestriction]
            public BufferLookup<Logic.WorldState> LookupWorldStates;
            void Execute([EntityIndexInQuery] int idx, in Root root, ref LogicAspect logic)
            {
                if (!logic.Def.IsSupportSystem(typeof(LogicEmitterMeteorite)))
                    return;

                var data = LookupWorldStates[root.Value];

                if (!logic.HasWorldState(Move.State.MoveDone, true) && data.HasWorldState(logic.Def, Move.State.MoveDone, true))
                {
                    logic.SetWorldState(Move.State.MoveDone, true);
                }

                if (logic.IsCurrentAction(Weapon.Action.Shoot))
                {
                    logic.SetWorldState(Weapon.State.HasAmo, false);
                    Writer.AddComponent<DeadTag>(idx, root.Value);
                }
            }
        }
    }
}