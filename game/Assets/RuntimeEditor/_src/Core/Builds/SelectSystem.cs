using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

using Game;
using Game.Model.Worlds;

using Unity.Jobs;

namespace Buildings.Environments
{
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    [UpdateBefore(typeof(PlaceSystem))]
    partial struct SelectSystem : ISystem
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

            m_Ground = new Plane(Vector3.up, Vector3.zero);
        }


        public void OnUpdate(ref SystemState state)
        {
            if (!Camera.main)
                return;
            if (m_QueryMap.IsEmpty)
                return;

            var input = m_Config.Value.MoveAction.ReadValue<Vector2>();
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);

            var aspect = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Map.Layers.Update(ref state);

            state.Dependency = new SystemJob()
            {
                Map = aspect,
                IsPlace = m_Config.Value.PlaceAction.triggered,
                Writer = ecb.AsParallelWriter(),
                Ground = m_Ground,
                Ray = Camera.main.ScreenPointToRay(input),
            }
            .Schedule(state.Dependency);
            state.Dependency.Complete();
        }

        partial struct SystemJob : IJob
        {
            private readonly BuildingContext.Var<IApiEditor> m_ApiEditor;
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Map;

            public EntityCommandBuffer.ParallelWriter Writer;
            public Plane Ground;
            public Ray Ray;
            public bool IsPlace;

            public void Execute()
            {
                if (!IsPlace || !Ground.Raycast(Ray, out float position)) return;
                
                Vector3 worldPosition = Ray.GetPoint(position);
                int2 pos = Map.Value.WordToMap(worldPosition) + Map.Value.Size / 2;

                if (Map.TryGetObject(m_ApiEditor.Value.CurrentLayer, pos, out var target))
                {
                    Writer.AddComponent(0, target, new SelectBuildingTag {Move = new Map.Transform{ Position = pos }});
                }
            }
        }
    }
}