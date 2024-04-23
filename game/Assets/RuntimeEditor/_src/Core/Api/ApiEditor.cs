using System.Collections.Generic;
using Unity.Entities;
using Common.Defs;
using Game;
using Game.Core.Spawns;
using Game.Core.Storages;
using Game.Model.Worlds;
using Game.Model.Logics;
using Game.Model.Stats;

using Reflex.Attributes;
using Reflex.Core;

using Unity.Mathematics;

namespace Buildings
{
    using Environments;
    using Game.Core;

    public class ApiEditor: Kernel, IApiEditor, IApiEditorHandler, IInitialization
    {
        [Inject] private static Container m_Container;
        
        private Dictionary<Entity, IPlaceHolder> m_Holders = new Dictionary<Entity, IPlaceHolder>();
        public TypeIndex CurrentLayer { get; private set; }

        public void Initialization()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = manager.CreateEntityQuery(ComponentType.ReadOnly<Map.Data>())
                .GetSingletonEntity();
            var aspect = manager.GetAspect<Map.Aspect>(entity);
        }

        public bool TryGetPlaceHolder(Entity entity, out IPlaceHolder holder)
        {
            return m_Holders.TryGetValue(entity, out holder);
        }

        public void AddEnvironment(IConfig config)
        {
            var ecb = GetBuffer();
            
            Spawn.Spawner.SpawnRequest(config, ecb, m_Container)
                .WithComponent<SelectBuildingTag>()
                .WithComponent<StorageTag>()
                .WithComponent<Map.Move>();
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

            public void Remove()
            {
                var ecb = GetBuffer();
                ecb.AddComponent<DeadTag>(Entity);
            }

            public void Place()
            {
                /*
                bool passable = Aspect.Value.Passable(pos);
                if (IsPlace && !passable)
                {
                    Debug.LogError($"Position {pos} in {placement.Value.Layer} layer is already occupied");
                }

                if (!passable || !IsPlace) return;
                    
                Writer.SetComponent(idx, entity, newMove);
                Writer.RemoveComponent<SelectBuildingTag>(idx, entity);
                m_ApiHandler.OnPlace(entity);
                */
            }

            private bool IsPlaceTaken(Map.Aspect aspect, TypeIndex layer, int2 pos, int2 size, Entity entity)
            {
                for (int i = 0, x = pos.x; i < size.x; i++, x++)
                {
                    for (int j = 0, y = pos.y; j < size.y; j++, y++)
                    {
                        var target = aspect.GetObject(layer, new int2(x, y)); 
                        if (target != Entity.Null && target != entity)
                            return true;
                    }
                }
                return false;
            }
        }
    }
}