using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Unity.Entities;
using System.Runtime.InteropServices;

namespace Common.Defs
{
    [Serializable]
    public struct Def<T>
    {
        private GCHandle m_ConfigHandle;
        public T Value => (T)m_ConfigHandle.Target;
        public Def(GCHandle handle)
        {
            m_ConfigHandle = handle;
        }
    }

    public static class DefExt
    {
        #region DefineableContexts
        public struct EntityManagerContext: IDefineableContext
        {
            private EntityManager m_Manager;

            public EntityManagerContext(EntityManager manager)
            {
                m_Manager = manager;
            }

            public DynamicBuffer<T> AddBuffer<T>(Entity entity) 
                where T : unmanaged, IBufferElementData
            {
                return m_Manager.AddBuffer<T>(entity);
            }
            public void AddComponentData<T>(Entity entity, T data) 
                where T : unmanaged, IComponentData
            {
                m_Manager.AddComponentData<T>(entity, data);
            }

            public void RemoveComponent<T>(Entity entity)
                where T : unmanaged, IComponentData
            {
                m_Manager.RemoveComponent<T>(entity);
            }

            public void AddComponentData(IDef def, Entity entity)
            {
                def.AddComponentData(entity, m_Manager);
            }

            public void RemoveComponentData<T>(IDef<T> def, Entity entity, T data) 
                where T : IDefineable
            {
                def.RemoveComponentData(entity, m_Manager, data);
            }
        }

        public struct IBakerContext : IDefineableContext
        {
            private readonly IBaker m_Manager;

            public IBakerContext(IBaker manager)
            {
                m_Manager = manager;
            }

            public DynamicBuffer<T> AddBuffer<T>(Entity entity)
                where T : unmanaged, IBufferElementData
            {
                return m_Manager.AddBuffer<T>(entity);
            }
            public void AddComponentData<T>(Entity entity, T data)
                where T : unmanaged, IComponentData
            {
                m_Manager.AddComponent<T>(entity, data);
            }

            public void RemoveComponent<T>(Entity entity)
                where T : unmanaged, IComponentData
            {
                throw new NotImplementedException();
            }
            public void AddComponentData(IDef def, Entity entity)
            {
                def.AddComponentData(entity, m_Manager);
            }
            public void RemoveComponentData<T>(IDef<T> def, Entity entity, T data)
                where T : IDefineable
            {
                throw new NotImplementedException();
            }
        }

        public struct WriterContext : IDefineableContext
        {
            private EntityCommandBuffer.ParallelWriter m_Manager;
            private readonly int m_SortKey;

            public WriterContext(EntityCommandBuffer.ParallelWriter manager, int sortKey)
            {
                m_Manager = manager;
                m_SortKey = sortKey;
            }

            public DynamicBuffer<T> AddBuffer<T>(Entity entity)
                where T : unmanaged, IBufferElementData
            {
                return m_Manager.AddBuffer<T>(m_SortKey, entity);
            }
            public void AddComponentData<T>(Entity entity, T data)
                where T : unmanaged, IComponentData
            {
                m_Manager.AddComponent<T>(m_SortKey, entity, data);
            }

            public void RemoveComponent<T>(Entity entity)
                where T : unmanaged, IComponentData
            {
                m_Manager.RemoveComponent<T>(m_SortKey, entity);
            }
            public void AddComponentData(IDef def, Entity entity)
            {
                def.AddComponentData(entity, m_Manager, m_SortKey);
            }
            public void RemoveComponentData<T>(IDef<T> def, Entity entity, T data)
                where T : IDefineable
            {
                def.RemoveComponentData(entity, m_Manager, m_SortKey, data);
            }
        }
        #endregion
        public static void AddComponentData(this IDef self, Entity entity, IDefineableContext context)
        {
            context.AddComponentData(self, entity);
        }

        public static void AddComponentData(this IDef self, Entity entity, EntityManager manager)
        {
            var data = CreateInstance(ref self);
            if (data is IDefineableCallback callback)
                callback.AddComponentData(entity, new EntityManagerContext(manager));
            manager.AddComponentIData(entity, ref data);
        }

        public static void AddComponentData(this IDef self, Entity entity, IBaker manager)
        {
            var data = CreateInstance(ref self);
            if (data is IDefineableCallback callback)
                callback.AddComponentData(entity, new IBakerContext(manager));
            manager.AddComponentIData(entity, ref data);
        }

        public static void AddComponentData(this IDef self, Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            var data = CreateInstance(ref self);
            if (data is IDefineableCallback callback)
                callback.AddComponentData(entity, new WriterContext(writer, sortKey));
            writer.AddComponentIData(entity, ref data, sortKey);
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, IDefineableContext context, T data)
            where T : IDefineable
        {
            context.RemoveComponentData(self, entity, data);
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey, T data)
            where T : IDefineable
        {
            var (target, _) = GetTargetType(self);
            writer.RemoveComponentIData(entity, target, sortKey);

            if (data is IDefineableCallback callback)
                callback.RemoveComponentData(entity, new WriterContext(writer, sortKey));
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, EntityManager manager, T data) 
            where T : IDefineable  
        {
            var (target, _) = GetTargetType(self);
            manager.RemoveComponentIData(entity, target);

            if (data is IDefineableCallback callback)
                callback.RemoveComponentData(entity, new EntityManagerContext(manager));
        }

        private static object CreateInstance(ref IDef def)
        {
            var (target, config) = GetTargetType(def);
            if (target == null)
                throw new NullReferenceException("Target type of def " + config.Name + " has a null target type");

            return InitDefineable(ref def, target, config);
        }

        public static unsafe object InitDefineable(ref IDef def, Type targetType, Type configType)
        {
            var handle = GCHandle.Alloc(def, GCHandleType.Pinned);
            var id = GCHandle.ToIntPtr(handle).ToInt64();

            if (!m_Initializes.TryGetValue(id, out DefInfo info))
            {
                Type defType = typeof(Def<>).MakeGenericType(configType);
                info = new DefInfo()
                {
                    Def = Activator.CreateInstance(defType, handle),
                    Initialize = targetType.GetConstructor(new Type[] { typeof(Def<>).MakeGenericType(configType) }),
                };
                m_Initializes.Add(id, info);
            }
            else
            {
                handle.Free();
            }
            if (info.Initialize == null)
                throw new MissingMethodException($"{targetType} constructor with parameter IntPtr not found!");

            return info.Initialize.Invoke(new object[] { info.Def });
        }

        private static (Type target, Type config) GetTargetType(IDef def)
        {
            Type findType = def.GetType();
            DefineableAttribute attr = findType.GetCustomAttribute<DefineableAttribute>(true);
            if (attr?.InstanceType != null)
                return (attr?.InstanceType, findType);

            if (m_Targets.TryGetValue(findType, out Type value) && value != null)
                return (value, findType);

            var defs = findType.FindInterfaces(
                (t, o) =>
                {
                    return (t.GetInterface(nameof(IDef)) != null && t.IsGenericType);

                }, null);

            if (defs.Length > 0)
                value = defs[0].GetGenericArguments()[0];

            m_Targets.Add(findType, value);

            return (value, findType);
        }

        private unsafe struct DefInfo
        {
            public object Def;
            public ConstructorInfo Initialize;
        }

        private static Dictionary<long, DefInfo> m_Initializes = new Dictionary<long, DefInfo>();
        private static Dictionary<Type, Type> m_Targets = new Dictionary<Type, Type>();
    }

    internal static class DefExtensions
    {
        public static void AddComponentIData(this EntityManager manager, Entity entity, ref object componentData)
        {
            Type DefType = componentData.GetType();

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.ManagerAdd == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();
                value.ManagerAdd = type.GetMethods()
                    .First(m => m.Name == "AddComponentData" && m.ReturnParameter.ParameterType == typeof(bool));

                value.ManagerAdd = value.ManagerAdd.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.ManagerAdd.Invoke(manager, new object[] { entity, componentData });
        }

        public static void AddComponentIData(this EntityCommandBuffer.ParallelWriter manager, Entity entity, ref object componentData, int sortKey)
        {
            Type DefType = componentData.GetType();

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.WriterAdd == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();
                value.WriterAdd = type.GetMethods()
                    .First(m => m.Name == "AddComponent" && m.GetParameters().Length == 3 && m.GetParameters()[1].ParameterType == typeof(Entity));

                value.WriterAdd = value.WriterAdd.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.WriterAdd.Invoke(manager, new object[] { sortKey, entity, componentData });
        }

        public static void AddComponentIData(this IBaker manager, Entity entity, ref object componentData)
        {
            Type DefType = componentData.GetType();

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.BakerAdd == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();
                value.BakerAdd = type.GetMethods()
                    .First(m => m.Name == "AddComponent" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(Entity));

                value.BakerAdd = value.BakerAdd.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.BakerAdd.Invoke(manager, new object[] { entity, componentData });
        }

        public static void RemoveComponentIData(this EntityCommandBuffer.ParallelWriter manager, Entity entity, Type typeComponent, int sortKey)
        {
            Type DefType = typeComponent;

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.WriterDel == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();

                value.WriterDel = type.GetMethods()
                    .First(m => m.Name == "RemoveComponent" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(Entity));

                value.WriterDel = value.WriterDel.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.WriterDel.Invoke(manager, new object[] { sortKey, entity });
        }

        public static void RemoveComponentIData(this EntityManager manager, Entity entity, Type typeComponent)
        {
            Type DefType = typeComponent;

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.ManagerDel == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();
                //RemoveComponent<T>(Entity e)
                value.ManagerDel = type.GetMethods()
                    .First(m => m.Name == "RemoveComponent" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Entity));

                value.ManagerDel = value.ManagerDel.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.ManagerDel.Invoke(manager, new object[] { entity });
        }

        protected class DefineableInfo
        {
            public MethodInfo ManagerAdd;
            public MethodInfo WriterAdd;
            public MethodInfo BakerAdd;

            public MethodInfo ManagerDel;
            public MethodInfo WriterDel;
            public MethodInfo BakerDel;
        }
        private static Dictionary<Type, DefineableInfo> m_Infos = new Dictionary<Type, DefineableInfo>();
    }
}