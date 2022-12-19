using System;

using Unity.Collections;
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
        void RemoveComponentData(IDef def, Entity entity, object data = null);

        T GetAspect<T>(Entity entity) where T : struct, IAspect, IAspectCreate<T>;
        T GetAspectRO<T>(Entity entity) where T : struct, IAspect, IAspectCreate<T>;

    }

    public interface IDefineableCallback
    {
        void AddComponentData(Entity entity, IDefineableContext context);
        void RemoveComponentData(Entity entity, IDefineableContext context);
    }
}