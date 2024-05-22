using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Transforms;
using Game;
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
        private static Map.Aspect m_MapAspect;

        public void OnCreate(ref SystemState state)
        {
            m_QueryMap = SystemAPI.QueryBuilder()
                .WithAspect<Map.Aspect>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Map.Placement>()
                .WithAllRW<Map.Move, LocalTransform>()
                .Build();
            m_Query.AddChangedVersionFilter(ComponentType.ReadOnly<Map.Move>());
            state.RequireForUpdate(m_Query);
        }

        public void OnUpdate(ref SystemState state)
        {
            m_MapAspect = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
            Map.Layers.Update(ref state);
            state.Dependency = new SystemJob()
            {
            }
            .ScheduleParallel(m_Query, state.Dependency);
        }
    
        partial struct SystemJob : IJobEntity
        {
            private void Execute(in Map.Placement placement, in Map.Move move, ref LocalTransform transform)
            {
                var newPos = m_MapAspect.Value.MapToWord(move.Position);
                var pivot = placement.Value.Pivot;

                transform.Rotation = quaternion.identity;
                transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(move.Rotation.y)));
                transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(math.radians(move.Rotation.x)));
                transform.Position = newPos - math.mul(transform.Rotation, pivot / 2);
            }
        }
    }
}