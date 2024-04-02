using System;
using Unity.Entities;
using Unity.Mathematics;

using static UnityEngine.Rendering.DebugUI;

namespace Unity.Transforms
{
    public struct WorldTransform
    {
        private ComponentLookup<LocalTransform> m_LookupTransforms;
        private ComponentLookup<LocalToWorld> m_LookupToWorlds;

        public LocalTransform Transform(Entity entity) => m_LookupTransforms[entity];
        public LocalToWorld ToWorld(Entity entity) => m_LookupToWorlds[entity];

        public RefRW<LocalTransform> GetTransformRefRW(Entity entity) => m_LookupTransforms.GetRefRW(entity);
        public RefRW<LocalToWorld> GetToWorldRefRW(Entity entity) => m_LookupToWorlds.GetRefRW(entity);
        public RefRO<LocalTransform> GetTransformRefRO(Entity entity) => m_LookupTransforms.GetRefRO(entity);
        public RefRO<LocalToWorld> GetToWorldRefRO(Entity entity) => m_LookupToWorlds.GetRefRO(entity);

        public WorldTransform(ref SystemState state, bool isReadOnly)
        {
            m_LookupTransforms = state.GetComponentLookup<LocalTransform>(isReadOnly);
            m_LookupToWorlds = state.GetComponentLookup<LocalToWorld>(isReadOnly);
        }
        
        public void Update(ref SystemState state)
        {
            m_LookupTransforms.Update(ref state);
            m_LookupToWorlds.Update(ref state);
        }

        public bool HasComponent(Entity entity)
        {
            return m_LookupTransforms.HasComponent(entity) && m_LookupToWorlds.HasComponent(entity);
        }
    }

    public static class WorldTransformExt
    {
        public static WorldTransform GetWorldTransformLookup(this ref SystemState state, bool isReadOnly = false)
        {
            return new WorldTransform(ref state, isReadOnly);
        }

        public static float3 Scale(this ref LocalToWorld self)
        {
            return new float3(self.Value.c0.x, self.Value.c1.y, self.Value.c2.z);
        }
    }
}
