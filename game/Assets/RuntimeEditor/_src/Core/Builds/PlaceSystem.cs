using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;

using Common.Core;
using Game;
using Game.Model.Stats;
using Game.Model.Worlds;

using Reflex.Attributes;

using Plane = UnityEngine.Plane;
using Ray = UnityEngine.Ray;

namespace Buildings.Environments
{
    public struct SelectBuildingTag : IComponentData
    {
        public Map.Move Move;
    }
    
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct PlaceSystem : ISystem
    {
        EntityQuery m_Query;
        EntityQuery m_QueryMap;
        Plane m_Ground;
        
        [Inject] private static Config m_Config;
        [Inject] private static IApiEditorHandler m_ApiHandler;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Map.Move, Map.Placement>()
                .WithAllRW<LocalTransform, SelectBuildingTag>()
                .WithNone<DeadTag>()
                .Build();

            m_Ground = new Plane(Vector3.up, Vector3.zero);
            state.RequireForUpdate(m_Query);
        }


        public void OnUpdate(ref SystemState state)
        {
            if (!Camera.main)
                return;

            var input = m_Config.MoveAction.ReadValue<Vector2>();
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

            var aspect = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Map.Layers.Update(ref state);

            state.Dependency = new SystemJob()
            {
                Aspect = aspect,
                IsPlace = m_Config.PlaceAction.triggered,
                IsCancel = m_Config.CancelAction.triggered,
                IsRotateX = m_Config.RorateXAction.triggered,
                IsRotateY = m_Config.RorateYAction.triggered,
                Writer = ecb.AsParallelWriter(),
                Ground = m_Ground,
                Ray = Camera.main.ScreenPointToRay(input),
            }
            .ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct SystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Aspect;

            public EntityCommandBuffer.ParallelWriter Writer;
            public Plane Ground;
            public Ray Ray;
            public bool IsPlace;
            public bool IsCancel;
            public bool IsRotateX;
            public bool IsRotateY;

            public void Execute([EntityIndexInQuery] int idx, in Entity entity, in Map.Move move,
                ref SelectBuildingTag selected, in Map.Placement placement, ref LocalTransform transform)
            {
                if (IsCancel)
                {
                    Aspect.SetObject(placement.Value.Layer, move.Position, Entity.Null);
                    Writer.AddComponent<DeadTag>(idx, entity);
                    m_ApiHandler.OnDestroy(entity);
                }
                else if (Ground.Raycast(Ray, out float position))
                {
                    var newMove = selected.Move;
                    if (IsRotateX)
                    {
                        newMove.Rotation.x += 45;
                        if (newMove.Rotation.x >= 360)
                            newMove.Rotation.x = 0;
                    }

                    if (IsRotateY)
                    {
                        newMove.Rotation.y += 180;
                        if (newMove.Rotation.y >= 360)
                            newMove.Rotation.y = 0;
                    }

                    Vector3 worldPosition = Ray.GetPoint(position);
                    
                    int2 pos = Aspect.Value.WordToMap(worldPosition) + Aspect.Value.Size / 2;
                    bool passable = Aspect.Value.Passable(pos);
                    if (!passable) return;
                    newMove.Position = pos;
                    
                    var newPos = Aspect.Value.MapToWord(pos);
                    var pivot = placement.Value.Pivot;

                    transform.Rotation = quaternion.identity;
                    transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(newMove.Rotation.y)));
                    transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(math.radians(newMove.Rotation.x)));
                    transform.Position = newPos + math.mul(transform.Rotation, pivot);

                    passable &= !IsPlaceTaken(Aspect, placement.Value.Layer, pos, placement.Value.Size, entity);
                    selected.Move = newMove; 

                    if (IsPlace && !passable)
                    {
                        Debug.LogError($"Position {pos} in {placement.Value.Layer} layer is already occupied");
                    }

                    if (!passable || !IsPlace) return;
                    
                    Writer.SetComponent(idx, entity, newMove);
                    Writer.RemoveComponent<SelectBuildingTag>(idx, entity);
                    m_ApiHandler.OnPlace(entity);
                }
            }

            private bool IsPlaceTaken(Map.Aspect aspect, TypeIndex layer, int2 pos, int2 size, Entity entity)
            {
                for (int i = 0, x = pos.x; i < size.x; i++, x++)
                {
                    for (int j = 0, y = pos.y; j < size.y; j++, y++)
                    {
                        var target = aspect.GetObject(layer, new int2(x, y)); 
                        if (target != Entity.Null && target != entity)
                            return true;
                    }
                }
                return false;
            }

        }
    }
}