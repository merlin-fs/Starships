using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
/*
namespace Game.Core.Saves
{
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial struct SaveSystem : ISystem
    {
        EntityQuery m_Query;
        EntityQuery m_QueryTargets;

        public void OnCreate(ref SystemState state)
        {
            m_QueryTargets = SystemAPI.QueryBuilder()
                .WithAll<SavedTag>()
                .Build();

            m_Query = SystemAPI.QueryBuilder()
                .WithAllRW<SaveTag>()
                .Build();

            state.RequireForUpdate(m_Query);
        }
        public void OnUpdate(ref SystemState state)
        {
            var entities = m_QueryTargets.ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
            var ecb = state.World.GetExistingSystemManaged<GameSpawnSystemCommandBufferSystem>().CreateCommandBuffer();
            var job = new SystemJob()
            {
                Entities = entities,

            };
            state.Dependency = job.ScheduleParallel(m_Query, handle);
            ecb.DestroyEntity(m_Query);
            entities.Dispose(state.Dependency);
        }

        partial struct SystemJob : IJobEntity
    }
}
*/