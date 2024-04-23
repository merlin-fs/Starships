using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;
using Game;
using Game.Core.Prefabs;
using Game.Core.Spawns;
using Game.Model.Worlds;

using UnityEngine;

namespace Buildings.Environments
{
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    [UpdateAfter(typeof(PlaceSystem))]
    partial struct MoveSystem : ISystem
    {
        private EntityQuery m_Query;
        private EntityQuery m_QueryMap;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Map.Placement>()
                .WithAllRW<Map.Move, LocalTransform>()
                .WithAny<SelectBuildingTag, Spawn.Tag>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Map.Move>());
            state.RequireForUpdate(m_Query);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (m_Query.IsEmpty) return;
            
            var map = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (move, transform, placement, context, entity) in SystemAPI.Query<Map.Move, RefRW<LocalTransform>, Map.Placement, PrefabInfo.ContextReference>()
                         .WithEntityAccess())
            {
                //var view = context.Value.Resolve<GameObject>();
                var newPos = map.Value.MapToWord(move.Position);
                var pivot = placement.Value.Pivot;

                var rotation = quaternion.identity;
                rotation = math.mul(rotation, quaternion.RotateX(math.radians(move.Rotation.y)));
                rotation = math.mul(rotation, quaternion.RotateY(math.radians(move.Rotation.x)));
                transform.ValueRW.Position = newPos + math.mul(rotation, pivot);
                transform.ValueRW.Rotation = rotation;
            }
            /*
            var aspect = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Map.Layers.Update(ref state);
            state.Dependency = new SystemJob()
            {
                Aspect = aspect,
            }
            .ScheduleParallel(m_Query, state.Dependency);
            //state.Dependency.Complete();
            */
        }
    
        /*
        partial struct SystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Aspect;

            private void Execute(in Entity entity, in Map.Placement placement, in Map.Move move, ref LocalTransform transform)
            {
                var newPos = Aspect.Value.MapToWord(move.Position);
                var pivot = placement.Value.Pivot;

                transform.Rotation = quaternion.identity;
                transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(move.Rotation.y)));
                transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(math.radians(move.Rotation.x)));
                transform.Position = newPos + math.mul(transform.Rotation, pivot);

                Aspect.SetObject(placement.Value.Layer, move.Position, entity);
            }
        }
        */
    }
}