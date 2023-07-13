using System;
using System.Reflection;

using Unity.Entities;

namespace Common.Defs
{
    public interface IDefinable { }

    public interface IDefineableContext
    {
        DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData;
        void AppendToBuffer<T>(Entity entity, T data) where T : unmanaged, IBufferElementData;
        void AddComponentData<T>(Entity entity, T data) where T : unmanaged, IComponentData;
        void RemoveComponent<T>(Entity entity) where T : unmanaged, IComponentData;
        void SetName(Entity entity, string name);
        Entity CreateEntity();
    }
    
    public interface IDefineableCallback
    {
        void AddComponentData(Entity entity, IDefineableContext context);
        void RemoveComponentData(Entity entity, IDefineableContext context);
    }
}