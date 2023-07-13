using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Common.Defs
{
    class DefHelper<T> where T : unmanaged, IDefinable, IComponentData
    {
        private Delegate m_Constructor;
        private ConcurrentDictionary<GCHandle, object> _objects = new ConcurrentDictionary<GCHandle, object>();
        private Type m_DefType;

        public DefHelper(IDef<T> def)
        {
            m_DefType = typeof(RefLink<>).MakeGenericType(def.GetType());
            var param = Expression.Parameter(m_DefType, "val");
            var lambda = Expression.Lambda(Expression.New(typeof(T).GetConstructor(new[] {m_DefType}), param), param);
            m_Constructor = lambda.Compile();
        }

        public T Create(ref IDef<T> def)
        {
            var handle = GCHandle.Alloc(def, GCHandleType.Pinned);
            if (!_objects.TryGetValue(handle, out var linkDef))
            {
                linkDef = Activator.CreateInstance(m_DefType, handle);
                _objects.TryAdd(handle, linkDef);
            }
            else 
                handle.Free();
            return (T)m_Constructor.DynamicInvoke(linkDef);
        }
    }

    public static class DefHelper
    {
        private static ConcurrentDictionary<Type, object> m_Helpers = new ConcurrentDictionary<Type, object>();

        public static void AddComponentData<T>(this IDef<T> self, Entity entity, IDefineableContext context)
            where T : unmanaged, IDefinable, IComponentData
        {
            var helper = GetHelper(self);
            var data = helper.Create(ref self);
            if (data is IDefineableCallback callback)
                callback.AddComponentData(entity, context);
            context.AddComponentData(entity, data);
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, T data, IDefineableContext context)
            where T : unmanaged, IDefinable, IComponentData
        {
            if (data is IDefineableCallback callback)
                callback.RemoveComponentData(entity, context);
            context.RemoveComponent<T>(entity);
        }

        private static DefHelper<T> GetHelper<T>(IDef<T> def)
            where T : unmanaged, IDefinable, IComponentData
        {
            var type = def.GetType();
            if (!m_Helpers.TryGetValue(type, out var helper))
            {
                helper = new DefHelper<T>(def);
                m_Helpers.TryAdd(type, helper);
            }
            return (DefHelper<T>)helper;
        }
    }
}