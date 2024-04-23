using Game.Core.Prefabs;
using Game.Model.Worlds;
using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

namespace Game.Model
{
    public partial struct Move
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct MoveHybridSystem : ISystem
        {
            private EntityQuery m_Query;
            private EntityQuery m_QueryMap;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<LocalTransform, PrefabInfo.ContextReference>()
                    .Build();
                
                m_QueryMap = SystemAPI.QueryBuilder()
                    .WithAspect<Map.Aspect>()
                    .Build();

                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                var map = SystemAPI.GetAspect<Map.Aspect>(m_QueryMap.GetSingletonEntity());
                var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
                var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
                foreach (var (transform, context, entity) in SystemAPI.Query<RefRO<LocalTransform>, PrefabInfo.ContextReference>()
                             .WithEntityAccess())
                {
                    var view = context.Value.Resolve<GameObject>();

                    view.transform.position = transform.ValueRO.Position;
                    view.transform.rotation = transform.ValueRO.Rotation;
                }
            }
        }
    }
}
