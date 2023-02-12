using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Game.Model.Logics
{
    using Game.Model.Weapons;

    using Stats;

    [UpdateInGroup(typeof(GamePartLogicSystemGroup))]
    public partial struct LogicEmitterMeteorite : Logic.IPartLogic
    {
        EntityQuery m_Query;
        ComponentLookup<Move> m_LookupMove;

        #region IPartLogic
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<WorldTransform>()
                .WithAll<Root>()
                .WithAll<Logic>()
                .WithNone<DeadTag>()
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<WorldTransform>());
            m_LookupMove = state.GetComponentLookup<Move>(true);
        }

        public void OnDestroy(ref SystemState state) { }
        
        public void OnUpdate(ref SystemState state)
        {
            m_LookupMove.Update(ref state);
            var ecb = state.World.GetExistingSystemManaged<GameLogicCommandBufferSystem>().CreateCommandBuffer(); 
            state.Dependency = new EmitterMoveJob()
            {
                Writer = ecb.AsParallelWriter(),
                LookupMove = m_LookupMove,
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
        #endregion
        public partial struct EmitterMoveJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            [ReadOnly]
            public ComponentLookup<Move> LookupMove;
            void Execute([EntityIndexInQuery] int idx, in Root root, ref LogicAspect logic, in TransformAspect transform)
            {
                if (!logic.Def.IsSupportSystem(typeof(LogicEmitterMeteorite)))
                    return;

                if (!logic.HasWorldState(Move.State.MoveDone, true))
                {
                    var data = LookupMove[root.Value];

                    var dt = math.distancesq(transform.WorldPosition, data.Position);
                    if (dt < 0.1f)
                    {
                        UnityEngine.Debug.Log($"[{logic.Self}] emitter move done {transform.WorldPosition}, target{data.Position}, dot {dt}");
                        logic.SetWorldState(Move.State.MoveDone, true);
                    }
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