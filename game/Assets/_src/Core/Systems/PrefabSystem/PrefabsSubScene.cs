using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Common.Core;
using Common.Defs;
using Unity.Burst;
using Game.Core.Defs;
using Game.Core.Repositories;
using Game.Model;
using Unity.Collections;
using Unity.Entities.Serialization;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
#endif


namespace Game.Core.Prefabs
{
#if UNITY_EDITOR
    
    public class PrefabsSubScene : MonoBehaviour
    {
        [SerializeField] private ReferenceSubScene m_SubScene;
        [SerializeField] private AddressableAssetGroup[] m_PrefabsGroup;

        public class _baker : Baker<PrefabsSubScene>
        {
            private ReferenceSubScene m_SubScene;
            private EntitySceneReference m_SceneReference;

            static T GetOrAddComponent<T>(GameObject uo) where T : Component
            {
                return uo.GetComponent<T>() ?? uo.AddComponent<T>();
            }

            public override void Bake(PrefabsSubScene authoring)
            {
                m_SubScene = authoring.m_SubScene;
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GetAssetOrScenePath(authoring.gameObject));
                m_SceneReference = new EntitySceneReference(sceneAsset);

                var self = GetEntityWithoutDependency();
                AddBuffer<PrefabBaking>(self);
                foreach (var iter in authoring.m_PrefabsGroup.SelectMany(p => p.entries))
                {
                    if (iter.MainAsset is GameObjectConfig config)
                    {
                        var root = GetEntity(config.PrefabObject, TransformUsageFlags.Dynamic);
                        BakeItem(config.ID, config.PrefabObject, self, root);
                        HierarchyConfig(config, self, root);
                    }
                    else
                    {
                        var prefab = (GameObject)iter.MainAsset;
                        GetEntity(prefab, TransformUsageFlags.Dynamic);
                        var component = GetOrAddComponent<PrefabEnvironmentAuthoring>(prefab);
                        component.ConfigID = ObjectID.Create(prefab.name);
                        component.Labels = iter.labels.ToArray();
                        
                        m_SubScene.Add(component.ConfigID, m_SceneReference);
                    }
                }
            }

            private void BakeItem(ObjectID id, GameObject prefab, Entity self, Entity root)
            {
                var entity = GetEntity(prefab, TransformUsageFlags.Dynamic);
                this.AppendToBuffer(self, new PrefabBaking 
                {
                    Entity = entity,
                    ConfigID = id,
                    Root = root,
                });

                m_SubScene.Add(id, m_SceneReference);
            }

            private void HierarchyConfig(GameObjectConfig config, Entity self, Entity root)
            {
                if (config is IConfigContainer container)
                {
                    foreach (var iter in container.Childs)
                    {
                        if (!iter.Enabled) continue;

                        if (iter.PrefabObject)
                        {
                            BakeItem(iter.Child.ID, iter.PrefabObject, self, root);
                        }
                        if (iter.Child is GameObjectConfig objectConfig)
                            HierarchyConfig(objectConfig, self, root);
                    }
                }
            }

        }
    }
    
    [TemporaryBakingType]
    internal struct PrefabBaking: IBufferElementData
    {
        public Entity Entity;
        public Entity Root;
        public ObjectID ConfigID;
    }
    
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct PrefabBakingSystem : ISystem
    {
        private EntityQuery m_Query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<PrefabBaking>()
                .WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab)
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var job = new SystemJob
            {
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
            job.Complete();
            ecb.RemoveComponent<PrefabBaking>(m_Query, EntityQueryCaptureMode.AtPlayback);

            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        private partial struct SystemJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Writer;

            [BurstCompile]
            private void Execute([EntityIndexInQuery] int idx, in Entity entity, 
                in DynamicBuffer<PrefabBaking> buffer)
            {
                foreach (var iter in buffer)
                {
                    Writer.AddComponent(idx, iter.Entity, new PrefabInfo()
                    {
                        ConfigID = iter.ConfigID,
                        Entity = iter.Entity,
                    });
                    Writer.AddComponent<PrefabInfo.BakedTag>(idx, iter.Entity);
                    Writer.AddComponent(idx, iter.Entity, new Root { Value = iter.Root });
                }
            }
        }
    }
#endif
}