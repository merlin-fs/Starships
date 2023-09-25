using System;

using Game.Model.Worlds;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Model
{
    using Logics;
    
    public partial struct Move
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct MoveSystem : ISystem
        {
            private EntityQuery m_Query;
            private EntityQuery m_QueryMap;

            public void OnCreate(ref SystemState state)
            {
                Map.PathFinder.Initialize();
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Logic.Aspect>()
                    .WithAspect<Aspect>()
                    .Build();

                m_QueryMap = SystemAPI.QueryBuilder()
                    .WithAspect<Map.Aspect>()
                    .Build();
                
                state.RequireForUpdate(m_Query);
            }

            public void OnDestroy(ref SystemState state)
            {
                Map.PathFinder.Dispose();
            }

            public void OnUpdate(ref SystemState state)
            {
                var map = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
                
                var job = new MoveJob()
                {
                    Delta = SystemAPI.Time.DeltaTime,
                    MapAspect = map,
                };
                state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct MoveJob : IJobEntity
            {
                [NativeSetThreadIndex] int m_ThreadIndex;
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Map.Aspect MapAspect;

                public float Delta;

                private void Execute(ref Move data, Aspect aspect, Logic.Aspect logic)
                {
                    if (logic.IsCurrentAction(Action.Init))
                    {
                        UnityEngine.Debug.Log($"{logic.Self} [Move] init {data.Position}, speed {data.Speed}");
                        aspect.LocalTransformRW.Position = data.Position;
                        aspect.LocalTransformRW = aspect.LocalTransformRW.Rotate(data.Rotation);
                        logic.SetWorldState(State.Init, true);
                        return;
                    }

                    if (logic.IsCurrentAction(Action.FindPath))
                    {
                        //logic.
                        //data.
                        //Map.Path.
                        //Map.GetCells()
                        //Map.PathFinder.Execute(m_ThreadIndex, null, MapAspect.Value, logic.Self, )
                        return;
                    }

                    if (logic.IsCurrentAction(Action.MoveToTarget) || logic.IsCurrentAction(Action.MoveToPosition))
                    {
                        //UnityEngine.Debug.Log($"[{logic.Self}] move to target {data.Position}, speed {data.Speed}, pos {transform.WorldPosition}");
                        float3 direction = data.Position - aspect.LocalTransformRO.Position;
                        var dt = math.distancesq(aspect.LocalTransformRO.Position, data.Position);
                        if (dt < 0.1f)
                        {
                            //UnityEngine.Debug.Log($"[{logic.Self}] move done {transform.WorldPosition}, target{data.Position}, dot {dt}");
                            logic.SetWorldState(State.MoveDone, true);
                        }
                        
                        var lookRotation = quaternion.LookRotationSafe(direction, math.up());
                        aspect.LocalTransformRW.Rotation = lookRotation;
                        aspect.LocalTransformRW.Position += math.normalize(direction) * Delta * data.Speed;
                        return;
                    }
                }
            }
        }
    }
}
