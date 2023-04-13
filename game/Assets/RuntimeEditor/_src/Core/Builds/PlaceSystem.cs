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

namespace Buildings.Environments
{
    public readonly struct SelectBuildingTag: IComponentData { }

    public struct BuildingPlace: IComponentData 
    {
        public int2 Size;
        public float2 Rorate;
        //public Bounds Bounds;
        public TypeIndex Layer;
    }

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct PlaceSystem : ISystem
    {
        EntityQuery m_Query;
        EntityQuery m_QueryMap;
        Plane m_Ground;
        BuildingContext.Var<Config> m_Config;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SelectBuildingTag>()
                .WithAllRW<BuildingPlace>()
                .WithAllRW<LocalTransform>()
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

            var aspect = SystemAPI.GetAspectRO<Map.Aspect>(m_QueryMap.GetSingletonEntity());
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
            private readonly BuildingContext.Var<IApiEditorHandler> m_ApiHandler;
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Aspect;

            public EntityCommandBuffer.ParallelWriter Writer;
            public Plane Ground;
            public Ray Ray;
            public bool IsPlace;
            public bool IsCancel;
            public bool IsRotateX;
            public bool IsRotateY;

            public void Execute([EntityIndexInQuery] int idx, in Entity entity, ref BuildingPlace bilding, ref LocalTransform transform)
            {
                if (IsCancel)
                {
                    Writer.DestroyEntity(idx, entity);
                    m_ApiHandler.Value.OnDestroy(entity);
                }
                else if (Ground.Raycast(Ray, out float position))
                {
                    if (IsRotateX)
                    {
                        bilding.Rorate.x += 45;
                        if (bilding.Rorate.x >= 360)
                            bilding.Rorate.x = 0;
                    }

                    if (IsRotateY)
                    {
                        bilding.Rorate.y += 180;
                        if (bilding.Rorate.y >= 360)
                            bilding.Rorate.y = 0;
                    }

                    Vector3 worldPosition = Ray.GetPoint(position);
                    
                    int2 pos = Aspect.Value.WordToMap(worldPosition) + Aspect.Value.Size / 2;
                    bool passable = Aspect.Value.Passable(pos);
                    if (!passable) return;
                    var newPos = Aspect.Value.MapToWord(pos);
                    //var pivot = -bilding.Bounds.extents / 2;
                    var pivot = - new float3(1.5f, 0.0625f, 1.5f) / 2;

                    transform.Rotation = quaternion.identity;
                    transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(bilding.Rorate.y)));
                    transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(math.radians(bilding.Rorate.x)));
                    transform.Position = newPos + math.mul(transform.Rotation, pivot);

                    passable &= !IsPlaceTaken(Aspect, bilding.Layer, pos, bilding.Size);

                    //flyingBuilding.SetTransparent(passable);
                    if (passable && IsPlace)
                    {
                        //Place object
                        Aspect.SetObject(bilding.Layer, pos, entity);
                        Writer.RemoveComponent<SelectBuildingTag>(idx, entity);
                        m_ApiHandler.Value.OnPlace(entity);
                    }
                }
            }

            private bool IsPlaceTaken(Map.Aspect aspect, TypeIndex layer, int2 pos, int2 size)
            {
                for (int i = 0, x = pos.x; i < size.x; i++, x++)
                {
                    for (int j = 0, y = pos.y; j < size.y; j++, y++)
                    {
                        if (aspect.GetObject(layer, new int2(x, y)) != Entity.Null)
                            return true;
                    }
                }
                return false;
            }

        }
    }
}