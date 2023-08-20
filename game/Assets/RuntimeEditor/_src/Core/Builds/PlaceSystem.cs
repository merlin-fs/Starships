using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;

using Common.Core;
using Game;
using Game.Model.Worlds;

using Plane = UnityEngine.Plane;
using Ray = UnityEngine.Ray;

namespace Buildings.Environments
{
    public struct SelectBuildingTag : IComponentData
    {
        public Map.Transform Move;
    }
    
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct PlaceSystem : ISystem
    {
        EntityQuery m_Query;
        EntityQuery m_QueryMap;
        Plane m_Ground;
        DiContext.Var<Config> m_Config;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Map.Transform, Map.Placement>()
                .WithAllRW<LocalTransform, SelectBuildingTag>()
                .Build();

            m_Ground = new Plane(Vector3.up, Vector3.zero);
            state.RequireForUpdate(m_Query);
        }


        public void OnUpdate(ref SystemState state)
        {
            if (!Camera.main)
                return;

            var input = m_Config.Value.MoveAction.ReadValue<Vector2>();
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

            var aspect = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Map.Layers.Update(ref state);

            state.Dependency = new SystemJob()
            {
                Aspect = aspect,
                IsPlace = m_Config.Value.PlaceAction.triggered,
                IsCancel = m_Config.Value.CancelAction.triggered,
                IsRotateX = m_Config.Value.RorateXAction.triggered,
                IsRotateY = m_Config.Value.RorateYAction.triggered,
                Writer = ecb.AsParallelWriter(),
                Ground = m_Ground,
                Ray = Camera.main.ScreenPointToRay(input),
            }
            .ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct SystemJob : IJobEntity
        {
            private readonly DiContext.Var<IApiEditorHandler> m_ApiHandler;
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Aspect;

            public EntityCommandBuffer.ParallelWriter Writer;
            public Plane Ground;
            public Ray Ray;
            public bool IsPlace;
            public bool IsCancel;
            public bool IsRotateX;
            public bool IsRotateY;

            public void Execute([EntityIndexInQuery] int idx, in Entity entity, in Map.Transform move,
                ref SelectBuildingTag selected, in Map.Placement placement, ref LocalTransform transform)
            {
                if (IsCancel)
                {
                    Aspect.SetObject(placement.Value.Layer, move.Position, Entity.Null);
                    Writer.DestroyEntity(idx, entity);
                    m_ApiHandler.Value.OnDestroy(entity);
                }
                else if (Ground.Raycast(Ray, out float position))
                {
                    var newMove = selected.Move;
                    if (IsRotateX)
                    {
                        newMove.Rorate.x += 45;
                        if (newMove.Rorate.x >= 360)
                            newMove.Rorate.x = 0;
                    }

                    if (IsRotateY)
                    {
                        newMove.Rorate.y += 180;
                        if (newMove.Rorate.y >= 360)
                            newMove.Rorate.y = 0;
                    }

                    Vector3 worldPosition = Ray.GetPoint(position);
                    
                    int2 pos = Aspect.Value.WordToMap(worldPosition) + Aspect.Value.Size / 2;
                    bool passable = Aspect.Value.Passable(pos);
                    if (!passable) return;
                    newMove.Position = pos;
                    
                    var newPos = Aspect.Value.MapToWord(pos);
                    var pivot = placement.Value.Pivot;

                    transform.Rotation = quaternion.identity;
                    transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(newMove.Rorate.y)));
                    transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(math.radians(newMove.Rorate.x)));
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
                    m_ApiHandler.Value.OnPlace(entity);
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