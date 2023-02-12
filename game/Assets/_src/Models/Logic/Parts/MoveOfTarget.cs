using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    using Units;
    using Stats;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct MoveOfTarget: Logic.IPartLogic
    {
        public EntityQuery m_Query;
        #region IPartLogic
        public EntityQuery Query => m_Query;
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Move>()
                .WithAll<Target>()
                .WithAll<Logic>()
                .WithAll<Unit>()
                .WithNone<DeadTag>()
                .Build();
        }

        public void OnDestroy(ref SystemState state) { }
        
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new LogicMoveJob()
            {
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
        #endregion
        public partial struct LogicMoveJob : IJobEntity
        {
            void Execute(ref Move data, in LogicAspect logic, in Target target, in UnitAspect unit)
            {
                if (!logic.Def.IsSupportSystem(typeof(MoveOfTarget)))
                    return;

                if (!logic.IsCurrentAction(Move.Action.MoveTo))
                    return;

                float3 pos = target.Value != Entity.Null
                    ? float3.zero
                    : target.WorldTransform.Position;

                data.Position = pos;
                data.Speed = unit.Stat(Unit.Stats.Speed).Value;
                //UnityEngine.Debug.Log($"[{logic.Self}] setup move to target {data.Position}, speed {data.Speed}");
            }
        }
    }
}