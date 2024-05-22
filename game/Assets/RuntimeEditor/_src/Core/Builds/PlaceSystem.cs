using System;
using System.Reflection.Ext;

using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;

using Game;
using Game.Model.Stats;
using Game.Model.Worlds;

using Reflex.Attributes;

using Plane = UnityEngine.Plane;
using Ray = UnityEngine.Ray;

namespace Buildings.Environments
{
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct PlaceSystem : ISystem
    {
        EntityQuery m_Query;
        EntityQuery m_QueryMap;
        Plane m_Ground;
        
        [Inject] private static Config m_Config;
        [Inject] private static IApiEditor m_ApiEditor;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Map.Placement, LocalTransform>()
                .WithAllRW<Editor.Selected, Map.Move>()
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

            public void Execute([EntityIndexInQuery] int idx, in Entity entity, ref Map.Move move,
                ref Editor.Selected selected, in Map.Placement placement)
            {
                if (!Ground.Raycast(Ray, out float position) || !m_ApiEditor.TryGetPlaceHolder(entity, out var placeHolder)) return;
                
                move.Rotation = selected.Position.Rotation;
                placeHolder.SetRotation(move.Rotation);

                float3 worldPosition = Ray.GetPoint(position);
                int2 pos = Aspect.Value.WordToMap(worldPosition) + Aspect.Value.Size / 2;

                bool passable = Aspect.Value.Passable(pos);
                placeHolder.SetCanPlace(passable, placement.Value.Layer);
                if (!passable) return;
                    
                move.Position = pos;
                placeHolder.SetPosition(move.Position);
                    
                passable &= !IsPlaceTaken(Aspect, placement.Value.Layer, pos, placement.Value.Size, entity);
                placeHolder.SetCanPlace(passable, placement.Value.Layer);
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