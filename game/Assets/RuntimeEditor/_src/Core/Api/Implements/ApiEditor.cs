using System;
using System.Threading;
using System.Collections.Generic;
using Unity.Entities;
using Common.Defs;
using Game;
using Game.Core;
using Game.Core.Events;
using Game.Model;

namespace Buildings
{
    using Environments;

    public class ApiEditor: IApiEditor, IKernel, IApiEditorHandler
    {
        public IEventHandler Events { get; }

        private EventDispatcher m_Dispatcher = EventDispatcher.CreateDefault();

        private Dictionary<Entity, IPlaceHolder> m_Holders = new Dictionary<Entity, IPlaceHolder>();

        public ApiEditor()
        {
            Events = new ProxyEvents(this);
        }

        void IKernel.SendEvent(EventBase e)
        {
            m_Dispatcher.Dispatch(e, this, DispatchMode.Default);
        }

        void IKernel.InvokeCallbacks(EventBase evt, PropagationPhase propagationPhase)
        {
            UnityMainThread.Context.Post((obj) =>
            {
                (Events as ProxyEvents).InvokeCallbacks(evt, propagationPhase);
            }, null);
        }

        public bool TryGetPlaceHolder(Entity entity, out IPlaceHolder holder)
        {
            return m_Holders.TryGetValue(entity, out holder);
        }

        public void AddEnvironment(IConfig config)
        {
            var ecb = GetBuffer();
            var entity = ecb.CreateEntity();
            ecb.AddBuffer<SpawnComponent>(entity);
            ecb.AppendToBuffer<SpawnComponent>(entity, ComponentType.ReadOnly<SelectBuildingTag>());
            ecb.AppendToBuffer<SpawnComponent>(entity, ComponentType.ReadOnly<SpawnEventTag>());

            ecb.AddComponent(entity, new NewSpawnMap
            {
                Prefab = config.Prefab,
            });
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
            m_Dispatcher.Dispatch(EventPlace.GetPooled(entity, EventPlace.eState.New), this, DispatchMode.Default);
        }

        void IApiEditorHandler.OnPlace(Entity entity)
        {
            m_Holders.Remove(entity);
            m_Dispatcher.Dispatch(EventPlace.GetPooled(entity, EventPlace.eState.Apply), this, DispatchMode.Default);
        }

        void IApiEditorHandler.OnDestroy(Entity entity)
        {
            m_Holders.Remove(entity);
            m_Dispatcher.Dispatch(EventPlace.GetPooled(entity, EventPlace.eState.Cancel), this, DispatchMode.Default);
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
                ecb.DestroyEntity(Entity);
            }
        }
    }
}