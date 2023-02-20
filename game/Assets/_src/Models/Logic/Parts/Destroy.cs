using System;
using Unity.Entities;

namespace Game.Model.Logics
{
    using Stats;
    using static UnityEngine.EventSystems.EventTrigger;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct Destroy: Logic.IPartLogic
    {
        public EntityQuery m_Query;
        #region IPartLogic
        public EntityQuery Query => m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Logic>()
                .Build();
        }

        public void OnDestroy(ref SystemState state) { }
        
        public void OnUpdate(ref SystemState state)
        {
            var ecb = state.World.GetExistingSystemManaged<GameLogicEndCommandBufferSystem>().CreateCommandBuffer();
            state.Dependency = new SystemJob()
            {
                Writer = ecb.AsParallelWriter(),
            }
            .ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }
        #endregion
        public partial struct SystemJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            public void Execute([EntityIndexInQuery] int idx, in Logic.Aspect logic)
            {
                if (logic.IsCurrentAction(Global.Action.Destroy) && logic.HasWorldState(Global.State.Dead, true))
                {
                    UnityEngine.Debug.Log($"{logic.Self} [Cleanup] set DeadTag");
                    Writer.AddComponent<DeadTag>(idx, logic.Self);
                    return;
                }
            }
        }
    }
}