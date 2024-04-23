using System;
using Buildings.Environments;
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
        public partial struct System : ISystem
        {
            private EntityQuery m_Query;
            private EntityQuery m_QueryMap;

            public void OnCreate(ref SystemState state)
            {
                Map.PathFinder.Initialize();
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Logic.Aspect>()
                    .WithAll<Move, LocalTransform>()
                    .WithNone<SelectBuildingTag>()
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
                Map.Layers.Update(ref state);
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

                private void Execute(ref LocalTransform transform, Move move, Logic.Aspect logic)
                {
                    /* logic
                    if (logic.IsCurrentAction(Action.Init))
                    {
                        UnityEngine.Debug.Log($"{logic.Self} [Move] init {aspect.Move.Position}, speed {aspect.Move.Speed}");
                        transform.Position = aspect.Move.Position;
                        transform = transform.Rotate(aspect.Move.Rotation);
                        logic.SetWorldState(State.Init, true);
                        return;
                    }

                    if (logic.IsCurrentAction(Action.FindPath))
                    {
                        //aspect.SetTarget(MapAspect.Value.MapToWord(aspect.Target), data.Speed);
                        /*
                        //UnityEngine.Debug.Log($"{logic.Self} [Move] FindPath {data.Position}, speed {data.Speed}");
                        var mapAspect = MapAspect;
                        var path = Map.PathFinder.Execute(m_ThreadIndex, (Map.Data map, Entity entity, int2? source, int2 target)=>
                            {
                                var passable = map.Passable(target);
                                if (!passable) return -1;
                                
                                passable &= mapAspect.GetObject<Map.Layers.Structure>(target) == Entity.Null;
                                passable &= mapAspect.GetObject<Map.Layers.Door>(target) == Entity.Null;
                                
                                return passable ? 1 : -1;
                            }, 
                            MapAspect.Value, logic.Self, aspect.Transform.Position, aspect.Target);
                        
                        var success = aspect.SetPath(path, MapAspect.Value);
                        data.Position = aspect.LocalTransformRO.Position; 
                        data.Rotation = aspect.LocalTransformRO.Rotation;
                        data.Travel = 0;
                        
                        logic.SetWorldState(State.PathFound, true);
                        return;
                    }

                    if (logic.IsCurrentAction(Action.MoveToPoint))
                    {
                        //UnityEngine.Debug.Log($"{logic.Self} [Move] MoveToPoint {data.Position}, speed {data.Speed}");
                        //if (aspect.Agent.IsStopped)
                        //logic.SetWorldState(State.MoveDone, true);
                        return;
                    }

                    if (logic.IsCurrentAction(Action.MoveToTarget) || logic.IsCurrentAction(Action.MoveToPosition))
                    {
                        //UnityEngine.Debug.Log($"{logic.Self} [Move] MoveToTarget {data.Position}, speed {data.Speed}");
                        float3 direction = aspect.Move.Position - transform.Position;
                        var dt = math.distancesq(transform.Position, aspect.Move.Position);
                        if (dt < 0.1f)
                        {
                            //UnityEngine.Debug.Log($"[{logic.Self}] move done {transform.WorldPosition}, target{data.Position}, dot {dt}");
                            logic.SetWorldState(State.MoveDone, true);
                        }
                        
                        var lookRotation = quaternion.LookRotationSafe(direction, math.up());
                        transform.Rotation = lookRotation;
                        transform.Position += math.normalize(direction) * Delta * aspect.Move.Speed;
                        return;
                    }
                    */
                }
            }
        }
    }
}
