using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using System.Security.Principal;

namespace Game.Model.Worlds.Bulds
{
    public readonly struct SelectBuildingTag: IComponentData { }

    public struct BuildingSize: IComponentData 
    {
        public int2 Size;
    }


    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    partial struct PlaceSystemSystem : ISystem
    {
        EntityQuery m_Query;
        EntityQuery m_QueryMap;
        Plane m_Ground;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SelectBuildingTag>()
                .WithAll<BuildingSize>()
                .WithAll<LocalTransform>()
                .Build();

            m_Ground = new Plane(Vector3.up, Vector3.zero);
            state.RequireForUpdate(m_Query);
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            if (!Camera.main)
                return;

            var map = SystemAPI.GetAspectRW<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new SystemJob()
            {
                Writer = ecb.AsParallelWriter(),
                Map = map,
                Ground = m_Ground,
                Ray = ray,
            }.ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct SystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Map;

            public EntityCommandBuffer.ParallelWriter Writer;

            public Plane Ground;
            public Ray Ray;

            public void Execute([EntityIndexInQuery] int idx, in Entity entity, in BuildingSize bilding, ref TransformAspect transform)
            {
                if (Ground.Raycast(Ray, out float position))
                {
                    Vector3 worldPosition = Ray.GetPoint(position);
                    int2 pos = Map.Value.WordToMap(worldPosition) + Map.Value.Size / 2;
                    bool passable = Map.Value.Passable(pos);
                    var newPos = Map.Value.MapToWord(pos);
                    if (!passable) return;
                    transform.WorldPosition = newPos;
                    passable &= IsPlaceTaken(Map, pos, bilding.Size);
                    //flyingBuilding.SetTransparent(passable);
                    if (passable && Input.GetMouseButtonDown(0))
                    {
                        Map.SetObject(pos, entity);
                        Writer.RemoveComponent<SelectBuildingTag>(idx, entity);
                        //PlaceFlyingBuilding(x, y);
                        UnityEngine.Debug.Log($"Raycast {worldPosition}, {newPos}, {pos}");
                    }
                }
            }

            private bool IsPlaceTaken(Map.Aspect Map, int2 pos, int2 size)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        if (Map.GetObject(pos) != Entity.Null)
                            return true;
                    }
                }
                return false;
            }

        }
    }
}