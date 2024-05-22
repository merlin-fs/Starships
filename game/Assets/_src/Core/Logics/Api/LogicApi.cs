using System;

using Game.Core;
using Game.Model.Logics;

using Reflex.Core;

using Unity.Collections;
using Unity.Entities;

namespace Game
{
    public class LogicApi : IInitialization
    {
        private Logic.LogicAfterSystem m_System;
        private EntityQuery m_QueryEnableAllLogics;
        private EntityQuery m_QueryDisableAllLogics;
        private EntityManager m_EntityManager;
        
        public void Initialization()
        {
            m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            m_QueryEnableAllLogics = m_EntityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] {typeof(Logic)},
                    Options = EntityQueryOptions.IncludeDisabledEntities,
                        
                });
            m_QueryDisableAllLogics = m_EntityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    Disabled = new ComponentType[] {typeof(Logic)},
                    Options = EntityQueryOptions.IncludeDisabledEntities,
                        
                });
            //m_QueryAllLogics.
            m_System = World.DefaultGameObjectInjectionWorld.EntityManager
                .World.GetOrCreateSystemManaged<Logic.LogicAfterSystem>();
        }

        public void SetWorldState<T>(Entity entity, T worldState, bool value)
            where T : struct, IConvertible
        {
            SetWorldState(entity, GoalHandle.FromEnum<T>(worldState, value));
        }

        public void SetWorldState(Entity entity, GoalHandle value)
        {
            m_System.ChangeWorldState(entity, value);
        }
        
        public void ActivateLogic(Entity entity, bool value)
        {
            var ecb = m_EntityManager.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();
            ecb.SetComponentEnabled<Logic>(entity, value);
            ecb.SetComponent(entity, new Logic.ChangeTag());
        }
        
        public void ActivateAllLogic(bool value)
        {
            var entities = value
                ? m_QueryDisableAllLogics.ToEntityArray(Allocator.Temp)
                : m_QueryEnableAllLogics.ToEntityArray(Allocator.Temp);
                
            var ecb = m_EntityManager.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                .CreateCommandBuffer();
            
            foreach (var entity in entities)
            {
                ecb.SetComponentEnabled<Logic>(entity, value);
                ecb.SetComponent(entity, new Logic.ChangeTag());
            }
            entities.Dispose();
        }
    }
}
