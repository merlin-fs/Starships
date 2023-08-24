using System;
using System.Threading;
using System.Collections.Generic;
using Unity.Entities;
using Common.Defs;
using Game;
using Game.Core;
using Game.Core.Events;
using Game.Core.Spawns;
using Game.Model;
using Game.Model.Worlds;
using Game.Core.Saves;
using Game.Model.Logics;
using Game.Model.Stats;

namespace Buildings
{
    using Environments;
    using Game.Core;

    public class ApiEditor: Kernel, IApiEditor, IApiEditorHandler
    {
        private Dictionary<Entity, IPlaceHolder> m_Holders = new Dictionary<Entity, IPlaceHolder>();

        public TypeIndex CurrentLayer { get; private set; }

        public bool TryGetPlaceHolder(Entity entity, out IPlaceHolder holder)
        {
            return m_Holders.TryGetValue(entity, out holder);
        }

        public void AddEnvironment(IConfig config)
        {
            var ecb = GetBuffer();
            var entity = ecb.CreateEntity();
            ecb.AddBuffer<Spawn.Component>(entity);

            ecb.AppendToBuffer<Spawn.Component>(entity, ComponentType.ReadOnly<SelectBuildingTag>());
            ecb.AppendToBuffer<Spawn.Component>(entity, ComponentType.ReadOnly<Spawn.EventTag>());
            ecb.AppendToBuffer<Spawn.Component>(entity, ComponentType.ReadOnly<SavedTag>());
            ecb.AppendToBuffer<Spawn.Component>(entity, ComponentType.ReadOnly<Map.Transform>());

            //var data = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Buildings.Environments.Building>(config.Prefab);
            //CurrentLayer = data.Def.Layer; 
            ecb.AddComponent(entity, new Spawn
            {
                Prefab = config.Prefab,
            });
        }

        public void SetLogicActive(bool value)
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var system = World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<Logic.System>(manager.World.GetExistingSystem<Logic.System>());
            system.Activate(value);
        }
        
        private static EntityCommandBuffer GetBuffer()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            return manager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
                .CreateCommandBuffer();
        }

        #region IApiEditorHandler
        void IApiEditorHandler.OnSpawn(Entity entity)
        {
            UnityEngine.Debug.Log($"{entity} OnSpawn");
            if (m_Holders.ContainsKey(entity)) return;
            var holder = new Placeholder(entity);
            m_Holders.Add(entity, holder);
            SendEvent(EventPlace.GetPooled(entity, EventPlace.State.New));
        }

        void IApiEditorHandler.OnPlace(Entity entity)
        {
            m_Holders.Remove(entity);
            SendEvent(EventPlace.GetPooled(entity, EventPlace.State.Apply));
        }

        void IApiEditorHandler.OnDestroy(Entity entity)
        {
            m_Holders.Remove(entity);
            SendEvent(EventPlace.GetPooled(entity, EventPlace.State.Cancel));
        }
        #endregion

        private readonly struct Placeholder : IPlaceHolder
        {
            public Entity Entity { get; }

            public Placeholder(Entity entity)
            {
                Entity = entity;
            }

            public void Cancel()
            {
                var ecb = GetBuffer();
                ecb.AddComponent<DeadTag>(Entity);
            }
        }
    }
}