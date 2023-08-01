using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;
using Game;
using Game.Core.Saves;
using Game.Model.Worlds;

namespace Buildings.Environments
{
    
    [Serializable, Saved]
    public struct Move : IComponentData
    {
        public int2 Position;
        public float2 Rorate;
    }

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
                .WithAll<Building>()
                .WithAllRW<Move>()
                .WithAllRW<LocalTransform>()
                .Build();

            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Move>());
            state.RequireForUpdate(m_Query);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (m_Query.IsEmpty) return;
            
            var aspect = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Map.Layers.Update(ref state);
            state.Dependency = new SystemJob()
            {
                Aspect = aspect,
            }
            .ScheduleParallel(m_Query, state.Dependency);
            //state.Dependency.Complete();
        }
    
        partial struct SystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction, NativeDisableUnsafePtrRestriction]
            public Map.Aspect Aspect;

            private void Execute(in Entity entity, in Building building, in Move move, ref LocalTransform transform)
            {
                var newPos = Aspect.Value.MapToWord(move.Position);
                var pivot = building.Def.Pivot;

                transform.Rotation = quaternion.identity;
                transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(move.Rorate.y)));
                transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(math.radians(move.Rorate.x)));
                transform.Position = newPos + math.mul(transform.Rotation, pivot);

                Aspect.SetObject(building.Def.Layer, move.Position, entity);
            }
        }
    }
}