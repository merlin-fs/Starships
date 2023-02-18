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
            state.Dependency = new SystemJob()
            {
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
        #endregion
        public partial struct SystemJob : IJobEntity
        {
            public void Execute(ref Move data, in LogicAspect logic, in Target target, in UnitAspect unit)
            {
                if (!logic.Def.IsSupportSystem(this))
                    return;

                if (logic.IsCurrentAction(Move.Action.MoveToTarget))
                {
                    data.Position = target.WorldTransform.Position;
                    data.Speed = unit.Stat(Unit.Stats.Speed).Value;
                    //UnityEngine.Debug.Log($"{logic.Self} [Logic part] MoveOfTarget set {data.Position}, {data.Speed}");
                    return;
                }

                if (logic.IsCurrentAction(Move.Action.MoveToPosition))
                {
                    data.Position = float3.zero;
                    data.Speed = unit.Stat(Unit.Stats.Speed).Value;
                    //UnityEngine.Debug.Log($"{logic.Self} [Logic part] MoveOfTarget set {data.Position}, {data.Speed}");
                }
            }
        }
    }
}