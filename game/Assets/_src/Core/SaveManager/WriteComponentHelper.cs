using System;
using System.Reflection;
using Unity.Entities;

namespace Game.Core.Saves
{
    public static class WriteComponentHelper
    {
        private static readonly MethodInfo m_AddComponent = typeof(WriteComponentHelper).GetMethod(nameof(InternalAddComponent), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo m_AddBuffer = typeof(WriteComponentHelper).GetMethod(nameof(InternalAddBuffer), BindingFlags.Static | BindingFlags.NonPublic);
        
        public static void AddComponent(this EntityCommandBuffer.ParallelWriter self, int sortKey, Entity entity, object data, Type dataType)
        {
            var type = data.GetType();
            var method = (type.IsArray) ? m_AddBuffer : m_AddComponent;
            method = method.MakeGenericMethod(dataType);
            if (type.IsArray && (data as object[]).Length == 0)
                return;
            method.Invoke(null, new[] {self, sortKey, entity, data});
        }
        
        private static void InternalAddComponent<T>(EntityCommandBuffer.ParallelWriter writer, int sortKey, Entity entity, T data)
            where T : unmanaged, IComponentData
        {
            writer.AddComponent(sortKey, entity, data);
        }

        private static void InternalAddBuffer<T>(EntityCommandBuffer.ParallelWriter writer, int sortKey, Entity entity, T[] data)
            where T : unmanaged, IBufferElementData
        {
            var buff = writer.AddBuffer<T>(sortKey, entity);
            foreach (var iter in data)
            {
                buff.Add(iter);
            } 
        }
    }
}