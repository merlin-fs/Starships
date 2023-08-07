using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Common.Defs;
using Common.Core;
using Game.Core.Repositories;
using Game.Model.Units;

namespace Game.Core.Prefabs
{
    public partial struct PrefabInfo
    {
        [UpdateInGroup(typeof(GameSpawnSystemGroup))]
        public partial struct System : ISystem
        {
            private EntityQuery m_QueryEnvironments;
            private EntityQuery m_QueryObjects;

            static private bool m_Done;

            static ConcurrentDictionary<Entity, IDefineableContext> m_Contexts;

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
                m_Contexts = new ConcurrentDictionary<Entity, IDefineableContext>();
                m_QueryEnvironments = SystemAPI.QueryBuilder()
                    .WithAll<PrefabInfo, BakedTag, BakedEnvironment, BakedLabel>()
                    .WithOptions(EntityQueryOptions.IncludePrefab)
                    .Build();

                m_QueryObjects = SystemAPI.QueryBuilder()
                    .WithAll<PrefabInfo, BakedTag>()
                    .WithNone<BakedEnvironment>()
                    .WithOptions(EntityQueryOptions.IncludePrefab)
                    .Build();
            }

            [BurstDiscard]
            public void OnUpdate(ref SystemState state)
            {
                if (m_QueryEnvironments.IsEmpty && m_QueryObjects.IsEmpty) return;
                
                m_Done = false;
                var system = SystemAPI.GetSingleton<GameSpawnSystemCommandBufferSystem.Singleton>();

                var environmentsHandle = new EnvironmentsJob
                {
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                }.ScheduleParallel(m_QueryEnvironments, new JobHandle());

                var objectsHandle = new ObjectsJob
                {
                    Writer = system.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                }.ScheduleParallel(m_QueryObjects, new JobHandle());
                state.Dependency = JobHandle.CombineDependencies(environmentsHandle, objectsHandle);
                
                new DoneJob().Schedule(state.Dependency);

                var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
                ecb.RemoveComponent<BakedTag>(m_QueryEnvironments, EntityQueryCaptureMode.AtPlayback);
                ecb.RemoveComponent<BakedEnvironment>(m_QueryEnvironments, EntityQueryCaptureMode.AtPlayback);
                ecb.RemoveComponent<BakedTag>(m_QueryObjects, EntityQueryCaptureMode.AtPlayback);
            }

            struct DoneJob : IJob
            {
                public void Execute()
                {
                    m_Contexts.Clear();
                    m_Done = true;
                }
            }

            partial struct EnvironmentsJob : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter Writer;
                readonly DIContext.Var<ObjectRepository> m_Repository;

                private void Execute([EntityIndexInQuery] int idx, in Entity entity,
                    in PrefabInfo prefab, in BakedEnvironment environment, in DynamicBuffer<BakedLabel> labels)
                {
                    var localLabels = labels.AsNativeArray()
                        .ToArray()
                        .Select(i => i.Label.ToString());

                    var def = new Structure.StructureDef 
                    {
                        Size = environment.Size,
                        Pivot = environment.Pivot,
                        Layer = TypeManager.GetTypeIndex(Type.GetType(environment.Layer.Value)),
                    };
                    var config = new StructureConfig(prefab.ConfigID, entity, def);
                    var context = new WriterContext(Writer, idx);
                    def.AddComponentData(entity, context);

                    context.SetName(entity, prefab.ConfigID.ToString());
                    m_Repository.Value.Insert(prefab.ConfigID, config, localLabels.ToArray());
                }
            }

            partial struct ObjectsJob : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter Writer;
                readonly DIContext.Var<ObjectRepository> m_Repository;

                void Execute([EntityIndexInQuery] int idx, in Entity entity, in PrefabInfo prefab)
                {
                    if (!m_Contexts.TryGetValue(entity, out IDefineableContext context))
                    {
                        context = new WriterContext(Writer, idx);
                        m_Contexts.TryAdd(entity, context);
                    }
                    var config = m_Repository.Value.FindByID(prefab.ConfigID);
                    
                    context.SetName(entity, prefab.ConfigID.ToString());
                    config.Configurate(entity, context);
                }
            }
        }
    }
}