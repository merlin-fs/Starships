using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Game;
using Game.Model.Worlds;
using Game.Core.Events;
using Common.Defs;
using Game.Core.Repositories;
using Common.Core;

namespace Buildings.Environments
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

        ComponentLookup<LocalTransform> m_LookupTransform;


        BuildingContext.Var<Config> m_Config;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<SelectBuildingTag>()
                .WithAll<BuildingSize>()
                .WithAllRW<LocalTransform>()
                .Build();

            m_LookupTransform = state.GetComponentLookup<LocalTransform>(false);
            m_Ground = new Plane(Vector3.up, Vector3.zero);
            state.RequireForUpdate(m_Query);
        }

        //[BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            if (!Camera.main)
                return;

            m_LookupTransform.Update(ref state);
            var input = m_Config.Value.MoveAction.ReadValue<Vector2>();
            var map = SystemAPI.GetAspectRW<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Ray ray = Camera.main.ScreenPointToRay(input);
            
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

            var IsPlace = m_Config.Value.PlaceAction.IsPressed();
            var IsCancel = m_Config.Value.CancelAction.IsPressed();

            state.Dependency = new SystemJob()
            {
                //LookupTransform = m_LookupTransform,
                IsPlace = IsPlace,
                IsCancel = IsCancel,
                Writer = ecb.AsParallelWriter(),
                Map = map,
                Ground = m_Ground,
                Ray = ray,
            }
            .ScheduleParallel(m_Query, state.Dependency);
            state.Dependency.Complete();
        }

        partial struct SystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Map;
            
            //[NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            //public ComponentLookup<LocalTransform> LookupTransform;

            public EntityCommandBuffer.ParallelWriter Writer;

            public Plane Ground;
            public Ray Ray;

            public bool IsPlace;
            public bool IsCancel;
            
            private BuildingContext.Var<IEventSender> m_EventSender;
            private BuildingContext.Var<IApiEditor> m_ApiEditor;

            public void Execute([EntityIndexInQuery] int idx, in Entity entity, in BuildingSize bilding)//, LocalTransform transform
            {
                if (IsCancel)
                {
                    Writer.DestroyEntity(idx, entity);
                    m_EventSender.Value.SendEvent(PlaceEvent.GetPooled(PlaceEvent.eState.Cancel));
                }
                else if (Ground.Raycast(Ray, out float position))
                {
                    Vector3 worldPosition = Ray.GetPoint(position);

                    int2 pos = Map.Value.WordToMap(worldPosition) + Map.Value.Size / 2;
                    bool passable = Map.Value.Passable(pos);
                    var newPos = Map.Value.MapToWord(pos);
                    if (!passable) return;

                    //var transform = LookupTransform.GetRefRW(entity, false);
                    ///transform.ValueRW.Position = newPos;

                    passable &= !IsPlaceTaken(Map, pos, bilding.Size);
                    //flyingBuilding.SetTransparent(passable);

                    if (passable && IsPlace)
                    {
                        Map.SetObject(pos, entity);
                        Writer.RemoveComponent<SelectBuildingTag>(idx, entity);
                        //PlaceFlyingBuilding(x, y);
                        UnityEngine.Debug.Log($"{entity} Raycast {worldPosition}, {newPos}, {pos}");
                        //EventSender.Value.SendEvent(PlaceEvent.GetPooled(PlaceEvent.eState.Apply));
                        var config = Repositories.Instance.GetRepo("Floor").FindByID(ObjectID.Create("Deck_Ceiling_01_snaps002"));
                        m_ApiEditor.Value.AddEnvironment(config);
                    }
                }
            }

            private bool IsPlaceTaken(Map.Aspect Map, int2 pos, int2 size)
            {
                for (int i = 0, x = pos.x; i < size.x; i++, x++)
                {
                    for (int j = 0, y = pos.y; j < size.y; j++, y++)
                    {
                        if (Map.GetObject(x, y) != Entity.Null)
                            return true;
                    }
                }
                return false;
            }

        }
    }
}