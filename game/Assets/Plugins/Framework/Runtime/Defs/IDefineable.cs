using System;
using Unity.Entities;

namespace Common.Defs
{
    public interface IDefineable
    {
    }
    
    public interface IDefineableContext
    {
        DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData;
        void AddComponentData<T>(Entity entity, T data) where T : unmanaged, IComponentData;
        void RemoveComponent<T>(Entity entity) where T : unmanaged, IComponentData;
        void AddComponentData(IDef def, Entity entity);
        void RemoveComponentData<T>(IDef<T> def, Entity entity, T data) where T : IDefineable;
    }

    public interface IDefineableCallback
    {
        void AddComponentData(Entity entity, IDefineableContext context);
        void RemoveComponentData(Entity entity, IDefineableContext context);
    }
}