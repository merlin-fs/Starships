using System;

using Buildings;

using Common.Core;

using Unity.Entities;

namespace Game.Core.Spawns
{
    public partial struct Spawn
    {
        [UpdateInGroup(typeof(GameSpawnSystemGroup))]
        partial struct EventSystem : ISystem
        {
            EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<EventTag>()
                    .Build();
                state.RequireForUpdate(m_Query);
            }

            partial struct SystemJob : IJobEntity
            {
                private IApiEditorHandler ApiHandler => Inject<IApiEditorHandler>.Value;

                void Execute(in Entity entity)
                {
                    ApiHandler.OnSpawn(entity);
                }
            }

            public void OnUpdate(ref SystemState state)
            {
                var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
                var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
                state.Dependency = new SystemJob{ }
                    .ScheduleParallel(m_Query, state.Dependency);
                
                ecb.RemoveComponent<EventTag>(m_Query, EntityQueryCaptureMode.AtRecord);
            }
        }
    }
}