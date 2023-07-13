using System;

using Buildings.Environments;

using Common.Core;
using Unity.Entities;
using Unity.Collections;
using Newtonsoft.Json.Linq;

namespace Game.Systems
{
    using Core.Saves;
    using Core.Repositories;
    using Core.Prefabs;
    using Model;
    using Views.Stats;
    
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    partial class SpawnLoadingSystem : SystemBase
    {
        private EntityQuery m_Query;
        private BufferLookup<StatView> m_ViewsLookup;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<LoadSpawnWorld>()
                .WithAll<SpawnComponent>()
                .Build();

            m_ViewsLookup = GetBufferLookup<StatView>();
            RequireForUpdate(m_Query);
        }

        partial struct SpawnJob : IJobEntity
        {
            private DIContext.Var<Repository> m_Repository;
            public EntityCommandBuffer.ParallelWriter Writer;
            [NativeDisableParallelForRestriction]
            public BufferLookup<StatView> ViewsLookup;

            void Execute([EntityIndexInQuery] int idx, in Entity entity, in LoadSpawnWorld spawn, in DynamicBuffer<SpawnComponent> components)
            {
                var prefabIndex = TypeManager.GetTypeIndex<PrefabRef>();
                var configId = spawn.Data.Value.Value<string>(prefabIndex.ToString());
                var config = m_Repository.Value.FindByID(configId);

                var inst = Writer.Instantiate(idx, config.Prefab);

                Writer.AddComponent(idx, inst, new PrefabRef {Prefab = config.Prefab});
                Writer.RemoveComponent<BakedPrefab>(idx, inst);
                
                foreach (var iter in spawn.Data.Value)
                {
                    var type = Type.GetType(((JProperty)iter).Name);
                    if (TypeManager.IsSystemType(type))
                        continue;
                    var component = iter.ToObject(type);
                    if (component != null)
                        Writer.AddComponent(idx, inst, component, type);
                }
                Writer.AddComponent<SpawnTag>(idx, inst);
                if (ViewsLookup.HasBuffer(entity))
                {
                    var views = ViewsLookup[entity];
                    var buff = Writer.AddBuffer<StatView>(idx, inst);
                    buff.CopyFrom(views);
                }
                foreach (var iter in components)
                    Writer.AddComponent(idx, inst, iter.ComponentType);
                
                Writer.DestroyEntity(idx, entity);
            }
        }

        protected override void OnUpdate()
        {
            m_ViewsLookup.Update(ref CheckedStateRef);
            var system = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
            var ecb = system.CreateCommandBuffer();
            Dependency = new SpawnJob()
            {
                ViewsLookup = m_ViewsLookup,
                Writer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, Dependency);

            system.AddJobHandleForProducer(Dependency);
        }
    }
}