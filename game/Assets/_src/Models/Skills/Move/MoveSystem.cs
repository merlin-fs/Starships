using System;
using Buildings.Environments;
using Game.Model.Worlds;

using Reflex.Core;
using Reflex.Attributes;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Game.Model
{
    using Logics;
    
    public partial struct Move
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct System : ISystem
        {
            [Inject] private static Container m_Container;
            
            private EntityQuery m_Query;
            private EntityQuery m_QueryMap;
            private WorldTransform m_LookupWorldTransform;
            private static Context.ContextManager<Context> m_ContextManager;

            public void OnCreate(ref SystemState state)
            {
                Map.PathFinder.Initialize();
                m_Query = SystemAPI.QueryBuilder()
                    .WithAspect<Logic.Aspect>()
                    .WithAll<Move, InternalData, LocalTransform>()
                    .Build();

                m_Query.SetChangedVersionFilter(ComponentType.ReadOnly<Move>());
                
                m_QueryMap = SystemAPI.QueryBuilder()
                    .WithAspect<Map.Aspect>()
                    .Build();
                
                state.RequireForUpdate(m_Query);
                m_LookupWorldTransform = new WorldTransform(ref state, false);
                m_ContextManager = new Context.ContextManager<Context>();
                m_ContextManager.Initialization(new Context.ContextGlobal(() => m_Container));
            }

            public void OnDestroy(ref SystemState state)
            {
                Map.PathFinder.Dispose();
            }

            public void OnUpdate(ref SystemState state)
            {
                var map = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
                Map.Layers.Update(ref state);
                m_LookupWorldTransform.Update(ref state);
                var system = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                
                var job = new MoveJob()
                {
                    LookupWorldTransform = m_LookupWorldTransform,
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    Delta = SystemAPI.Time.DeltaTime,
                    MapAspect = map,
                };
                state.Dependency = job.ScheduleParallel(m_Query, state.Dependency);
            }

            partial struct MoveJob : IJobEntity
            {
                [NativeDisableParallelForRestriction]
                public WorldTransform LookupWorldTransform;
                public EntityCommandBuffer.ParallelWriter Writer;
                
                [NativeSetThreadIndex] int m_ThreadIndex;
                [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
                public Map.Aspect MapAspect;

                public float Delta;

                private void Execute([EntityIndexInQuery] int idx, Entity entity, Logic.Aspect logic, 
                    Move move, ref InternalData data)
                {
                    if (move.Query.HasFlag(QueryFlags.Rotate))
                    {
                        quaternion newRotation;
                        if (move.Query.HasFlag(QueryFlags.Target))
                        {
                            var target = LookupWorldTransform.ToWorld(move.Target);
                            var self = LookupWorldTransform.ToWorld(entity);
                            float3 direction = target.Position - self.Position;
                            newRotation = quaternion.LookRotationSafe(direction, math.up());
                        }
                        else
                            newRotation = move.Rotation;

                        var root = LookupWorldTransform.Transform(logic.Root);
                        var transform = LookupWorldTransform.GetTransformRefRW(entity);
                        ref var rotation = ref transform.ValueRW.Rotation;
                        newRotation = root.InverseTransformRotation(newRotation);
                        
                        var newEuler = math.Euler(newRotation);
                        var euler = math.Euler(rotation);

                        if (data.Time == 0)
                            data.Store = euler; 
                        rotation = quaternion.Euler(math.lerp(data.Store, newEuler, data.Time));
                        data.Time += Delta * move.Travel;

                        move.Rotation = newRotation;
                        var dot = math.abs(math.dot(rotation, newRotation));
                        if (data.Time < 1f)
                        {
                            Writer.SetComponent(idx, entity, move);
                        }
                        else
                        {
                            rotation = newRotation;
                            data.Time = 0;
                        }
                    }
                    
                    var context = m_ContextManager.Get(
                        new Context.ContextRecord(
                            entity, 
                            Delta, 
                            StructRef.Get(ref Writer), 
                            idx, 
                            move, 
                            LookupWorldTransform));
                    logic.ExecuteAction(context);
                    m_ContextManager.Release(context);
                    
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
