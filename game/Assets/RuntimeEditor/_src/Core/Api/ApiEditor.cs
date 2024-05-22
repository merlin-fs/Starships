using System;
using System.Collections.Generic;
using Unity.Entities;
using Common.Defs;
using Game;
using Game.Core;
using Game.Core.Spawns;
using Game.Core.Storages;
using Game.Model.Worlds;
using Game.Model.Logics;
using Game.Model.Stats;

using Reflex.Attributes;
using Reflex.Core;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Buildings
{

    public class ApiEditor: Kernel, IApiEditor, IInitialization
    {
        [Inject] private Container m_Container;
        [Inject] private static Config m_Config;
        [Inject] private LogicApi m_LogicApi;

        private EntityManager m_EntityManager;
        
        private readonly Dictionary<int, PlaceHolder> m_Holders = new();
        public TypeIndex CurrentLayer { get; private set; }

        public void Initialization()
        {
            m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = m_EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Map.Data>())
                .GetSingletonEntity();
            Events.RegisterCallback<EventEditorSpawn>(e => DoSpawn(e.Entity, e.Old));
        }

        public bool TryGetPlaceHolder(Entity entity, out IPlaceHolder holder)
        {
            holder = null;
            if (!m_Holders.TryGetValue(entity.Index, out var placeHolder)) return false;
            holder = placeHolder;
            return true;
        }

        public IPlaceHolder AddObject(IConfig config)
        {
            var ecb = GetBuffer();
            var builder = Spawn.Spawner.Spawn(config, ecb, m_Container)
                .WithComponent<Editor.Selected>()
                .WithComponent<StorageTag>()
                .WithView()
                .WithComponent<Map.Move>();
            builder.WithComponent(new Editor.Spawn {Index = builder.Entity.Index});
            var placeHolder = new PlaceHolder(this, builder.Entity);
            m_Holders.Add(builder.Entity.Index, placeHolder);
            return placeHolder;
        }

        public bool Remove(IPlaceHolder placeHolder)
        {
            if (placeHolder == null) return false;
            var entity = ((PlaceHolder)placeHolder).Entity;
            if (!m_Holders.Remove(entity.Index, out var holder)) return false;
            holder.Remove();
            holder.Dispose();
            DoDestroy(entity);
            return true;
        }

        public bool Place(IPlaceHolder placeHolder)
        {
            if (placeHolder == null) return false;
            var entity = ((PlaceHolder)placeHolder).Entity;
            if (!m_Holders.Remove(entity.Index, out var holder)) return false;

            holder.Dispose();
            DoPlace(entity);
            return true;
        }

        public void SetLogicActive(bool value)
        {
            m_LogicApi.ActivateAllLogic(value);
        }
        
        private EntityCommandBuffer GetBuffer()
        {
            return m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
                .CreateCommandBuffer();
        }

        private void DoSpawn(Entity entity, int old)
        {
            if (!m_Holders.TryGetValue(old, out var placeHolder)) return;
            placeHolder.SetEntity(entity);
            m_Holders.Remove(old);
            m_Holders[entity.Index] = placeHolder;
            SendEvent(EventPlace.GetPooled(entity, EventPlace.State.New));
        }

        private void DoPlace(Entity entity)
        {
            SendEvent(EventPlace.GetPooled(entity, EventPlace.State.Apply));
        }

        private void DoDestroy(Entity entity)
        {
            SendEvent(EventPlace.GetPooled(entity, EventPlace.State.Cancel));
        }

        private class PlaceHolder : IPlaceHolder, IDisposable
        {
            public Entity Entity { get; private set; }
            private readonly ApiEditor m_Editor;
            private Map.Move m_Transform;
            private bool m_CanPlace;
            private TypeIndex m_Layer;

            public PlaceHolder(ApiEditor apiEditor, Entity entity)
            {
                m_Editor = apiEditor;
                Entity = entity;
                RegistryActions();
            }

            public void SetPosition(int2 position) => m_Transform.Position = position;
            public void SetRotation(float2 rotation) => m_Transform.Rotation = rotation;

            public void SetCanPlace(bool value, TypeIndex layer)
            {
                m_CanPlace = value;
                m_Layer = layer;
            }

            private void RegistryActions()
            {
                m_Config.RorateXAction.performed += OnRotateX;
                m_Config.RorateXAction.started += OnRotateX;
                m_Config.RorateXAction.canceled += OnRotateX;

                m_Config.RorateYAction.performed += OnRotateY;
                m_Config.RorateYAction.started += OnRotateY;
                m_Config.RorateYAction.canceled += OnRotateY;

                m_Config.PlaceAction.performed += OnApplyPlace;
                m_Config.PlaceAction.started += OnApplyPlace;
                m_Config.PlaceAction.canceled += OnApplyPlace;
                
                m_Config.CancelAction.performed += OnCancel;
                m_Config.CancelAction.started += OnCancel;
                m_Config.CancelAction.canceled += OnCancel;
            }

            private void OnCancel(InputAction.CallbackContext context)
            {
                if (!context.performed) return;

                m_Editor.Remove(this);
            }
            
            private void OnApplyPlace(InputAction.CallbackContext context)
            {
                if (!context.performed) return;
                if (!m_CanPlace) return;

                var ecb = m_Editor.GetBuffer();
                ecb.RemoveComponent<Editor.Selected>(Entity);
                m_Editor.Place(this);
            }

            private void OnRotateX(InputAction.CallbackContext context)
            {
                if (!context.performed) return;

                m_Transform.Rotation.x += 45;
                if (m_Transform.Rotation.x >= 360)
                    m_Transform.Rotation.x = 0;
                
                var data = new Editor.Selected {Position = m_Transform};
                var ecb = m_Editor.GetBuffer();
                ecb.SetComponent(Entity, data);
            }

            private void OnRotateY(InputAction.CallbackContext context)
            {
                if (!context.performed) return;

                m_Transform.Rotation.y += 180;
                if (m_Transform.Rotation.y >= 360)
                    m_Transform.Rotation.y = 0;

                var data = new Editor.Selected {Position = m_Transform};
                var ecb = m_Editor.GetBuffer();
                ecb.SetComponent(Entity, data);
                
            }
            
            public void SetEntity(Entity entity) => Entity = entity;
            
            public void Remove()
            {
                var ecb = m_Editor.GetBuffer();
                ecb.AddComponent<DeadTag>(Entity);
            }

            public void Dispose()
            {
                m_Config.RorateXAction.performed -= OnRotateX;
                m_Config.RorateXAction.started -= OnRotateX;
                m_Config.RorateXAction.canceled -= OnRotateX;

                m_Config.RorateYAction.performed -= OnRotateY;
                m_Config.RorateYAction.started -= OnRotateY;
                m_Config.RorateYAction.canceled -= OnRotateY;

                m_Config.PlaceAction.performed -= OnApplyPlace;
                m_Config.PlaceAction.started -= OnApplyPlace;
                m_Config.PlaceAction.canceled -= OnApplyPlace;

                m_Config.CancelAction.performed -= OnCancel;
                m_Config.CancelAction.started -= OnCancel;
                m_Config.CancelAction.canceled -= OnCancel;
            }
        }
    }
}