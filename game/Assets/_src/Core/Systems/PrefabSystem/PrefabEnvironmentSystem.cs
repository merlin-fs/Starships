using System;
using System.Threading.Tasks;
using System.Linq;

using Buildings.Environments;

using Unity.Entities;
using Unity.Burst;
using Common.Core;
using Common.Defs;

namespace Game.Core.Prefabs
{
    using Repositories;

    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial struct PrefabEnvironmentSystem : ISystem
    {
        EntityQuery m_Query;
        private static bool m_Done;

        public Task<bool> IsDone()
        {
            return Task.Run(() =>
            {
                while (!m_Done)
                    Task.Yield();
                return m_Done;
            });
        }

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<BakedPrefabTag>()
                .WithAll<BakedPrefab>()
                .WithAll<BakedEnvironment>()
                .WithAll<BakedPrefabLabel>()
                .WithOptions(EntityQueryOptions.IncludePrefab)
                .Build();

            state.RequireForUpdate(m_Query);
        }


        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            m_Done = false;
            var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();
            var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
            
            state.Dependency = new PrefabJob()
            {
                Writer = ecb.AsParallelWriter(),
                
            }.ScheduleParallel(m_Query, state.Dependency);
            state.CompleteDependency();
            ecb.RemoveComponent<BakedPrefabTag>(m_Query);
            m_Done = true;
        }

        partial struct PrefabJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            readonly DIContext.Var<ObjectRepository> m_Repository;

            private void Execute([EntityIndexInQuery] int idx, in Entity entity, 
                in BakedPrefab bakedPrefab, in BakedEnvironment environment, in DynamicBuffer<BakedPrefabLabel> labels)
            {
                var localLabels = labels.AsNativeArray()
                    .ToArray()
                    .Select(i => i.Label.ToString());
                var def = new Building.BuildingDef
                {
                    Size = environment.Size, 
                    Pivot = environment.Pivot,
                    Layer = TypeManager.GetTypeIndex(Type.GetType(environment.Layer.Value)),
                };
                var config = new BuildingConfig(bakedPrefab.ConfigID, entity, def);
                
                var context = new WriterContext(Writer, idx);
                Writer.RemoveComponent<BakedEnvironment>(idx, entity);
                def.AddComponentData(entity, context);
                
                
                m_Repository.Value.Insert(bakedPrefab.ConfigID, config, localLabels.ToArray());
            }
        }
    }
}