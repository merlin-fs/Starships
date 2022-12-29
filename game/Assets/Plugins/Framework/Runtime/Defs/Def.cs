using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Unity.Entities;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Common.Defs
{
    [Serializable]
    public struct Def<T>
        where T : IDef
    {
        private BlobAssetReference<GCHandle> m_RefHandle;

        public T Value 
        { 
            get {
                
                if (!m_RefHandle.Value.IsAllocated)
                {

                }
                return (T)m_RefHandle.Value.Target;
            }
        } 

        public Def(BlobAssetReference<GCHandle> handle)
        {
            m_RefHandle = handle;
        }
    }

    public static class DefExt
    {
        #region DefineableContexts
        public struct EntityManagerContext: IDefineableContext
        {
            private EntityManager m_Manager;
            private NativeHashMap<Hash128, Entity> m_Childs;
            public EntityManagerContext(EntityManager manager, NativeHashMap<Hash128, Entity> childs = default)
            {
                m_Manager = manager;
                m_Childs = childs;
            }

            public Entity FindEntity(Hash128 prefabId)
            {
                if (!m_Childs.IsCreated)
                    return default;
                m_Childs.TryGetValue(prefabId, out Entity entity);
                return entity;
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
                def.AddComponentData(entity, m_Manager, this);
            }

            public void RemoveComponentData<T>(IDef<T> def, Entity entity, T data) 
                where T : IDefineable
            {
                def.RemoveComponentData(entity, m_Manager, data, this);
            }

            public void SetName(Entity entity, string name)
            {
                FixedString64Bytes fs = default;
                FixedStringMethods.CopyFromTruncated(ref fs, name);
                m_Manager.SetName(entity, fs);
            }
        }

        public struct CommandBufferContext : IDefineableContext
        {
            private EntityCommandBuffer m_Manager;
            private NativeHashMap<Hash128, Entity> m_Childs;

            public CommandBufferContext(EntityCommandBuffer manager, NativeHashMap<Hash128, Entity> childs = default)
            {
                m_Manager = manager;
                m_Childs = childs;
            }

            public Entity FindEntity(Hash128 prefabId)
            {
                if (!m_Childs.IsCreated)
                    return default;
                m_Childs.TryGetValue(prefabId, out Entity entity);
                return entity;
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
                m_Manager.RemoveComponent<T>(entity);
            }
            public void AddComponentData(IDef def, Entity entity)
            {
                def.AddComponentData(entity, m_Manager, this);
            }
            public void RemoveComponentData<T>(IDef<T> def, Entity entity, T data)
                where T : IDefineable
            {
                def.RemoveComponentData(entity, m_Manager, data, this);
            }

            public void SetName(Entity entity, string name)
            {
                FixedString64Bytes fs = default;
                FixedStringMethods.CopyFromTruncated(ref fs, name);
                m_Manager.SetName(entity, fs);
            }
        }

        public struct WriterContext : IDefineableContext
        {
            private EntityCommandBuffer.ParallelWriter m_Manager;
            private readonly int m_SortKey;
            private NativeHashMap<Hash128, Entity> m_Childs;

            public WriterContext(EntityCommandBuffer.ParallelWriter manager, int sortKey, NativeHashMap<Hash128, Entity> childs = default)
            {
                m_Manager = manager;
                m_SortKey = sortKey;
                m_Childs = childs;
            }

            public Entity FindEntity(Hash128 prefabId)
            {
                if (!m_Childs.IsCreated)
                    return default;
                m_Childs.TryGetValue(prefabId, out Entity entity);
                return entity;
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
                def.AddComponentData(entity, m_Manager, m_SortKey, this);
            }
            public void RemoveComponentData<T>(IDef<T> def, Entity entity, T data)
                where T : IDefineable
            {
                def.RemoveComponentData(entity, m_Manager, m_SortKey, data, this);
            }
            
            public void SetName(Entity entity, string name)
            {
                FixedString64Bytes fs = default;
                FixedStringMethods.CopyFromTruncated(ref fs, name);
                m_Manager.SetName(m_SortKey, entity, fs);
            }
        }
        #endregion
        #region ext entities
        public static void AddComponentData(this IDef self, Entity entity, IDefineableContext context)
        {
            context.AddComponentData(self, entity);
        }

        public static void AddComponentData(this IDef self, Entity entity, EntityManager manager, IDefineableContext context = null)
        {
            var data = CreateInstance(ref self);
            if (data is IDefineableCallback callback)
            {
                context ??= new EntityManagerContext(manager);
                callback.AddComponentData(entity, context);
            }
            manager.AddComponentIData(entity, ref data);
        }

        public static void AddComponentData(this IDef self, Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey, IDefineableContext context = null)
        {
            var data = CreateInstance(ref self);
            if (data is IDefineableCallback callback)
            {
                context ??= new WriterContext(writer, sortKey);
                callback.AddComponentData(entity, context);
            }
            writer.AddComponentIData(entity, ref data, sortKey);
        }

        public static void AddComponentData(this IDef self, Entity entity, EntityCommandBuffer writer, IDefineableContext context = null)
        {
            var data = CreateInstance(ref self);
            if (data is IDefineableCallback callback)
            {
                context ??= new CommandBufferContext(writer);
                callback.AddComponentData(entity, context);
            }
            writer.AddComponentIData(entity, ref data);
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, IDefineableContext context, T data)
            where T : IDefineable
        {
            context.RemoveComponentData(self, entity, data);
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey, T data, IDefineableContext context = null)
            where T : IDefineable
        {
            if (data is IDefineableCallback callback)
            {
                context ??= new WriterContext(writer, sortKey);
                callback.RemoveComponentData(entity, context);
            }
            var (target, _) = GetTargetType(self);
            writer.RemoveComponentIData(entity, target, sortKey);
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, EntityCommandBuffer writer, T data, IDefineableContext context = null)
            where T : IDefineable
        {
            if (data is IDefineableCallback callback)
            {
                context ??= new CommandBufferContext(writer);
                callback.RemoveComponentData(entity, context);
            }
            var (target, _) = GetTargetType(self);
            writer.RemoveComponentIData(entity, target);
        }

        public static void RemoveComponentData<T>(this IDef<T> self, Entity entity, EntityManager manager, T data, IDefineableContext context = null) 
            where T : IDefineable  
        {
            if (data is IDefineableCallback callback)
            {
                context ??= new EntityManagerContext(manager);
                callback.RemoveComponentData(entity, context);
            }
            var (target, _) = GetTargetType(self);
            manager.RemoveComponentIData(entity, target);
        }
        #endregion
        
        private static object CreateInstance(ref IDef def)
        {
            var (target, config) = GetTargetType(def);

            return target == null
                ? throw new NullReferenceException("Target type of def " + config.Name + " has a null target type")
                : InitDefineable(ref def, target, config);
        }

        public static unsafe BlobAssetReference<GCHandle> GetHandle(this IDef def)
        {
            var id = def.Initialize();
            return m_Initializes[id].Handle;
        }

        private unsafe static long Initialize(this IDef def)
        {
            var handle = GCHandle.Alloc(def, GCHandleType.Pinned);
            var id = GCHandle.ToIntPtr(handle).ToInt64();

            if (!m_Initializes.ContainsKey(id))
            {
                using var builder = new BlobBuilder(Allocator.Temp);
                ref GCHandle refHandle = ref builder.ConstructRoot<GCHandle>();
                UnsafeUtility.CopyStructureToPtr(ref handle, UnsafeUtility.AddressOf(ref refHandle));

                var hh = builder.CreateBlobAssetReference<GCHandle>(Allocator.Persistent);
                var info = new DefInfo()
                {
                    Handle = hh,
                    Def = null,
                    Initialize = null,
                };
                m_Initializes.Add(id, info);
            }
            else
            {
                handle.Free();
            }
            return id;
        }

        public static unsafe object InitDefineable(ref IDef def, Type targetType, Type configType)
        {
            var id = def.Initialize();

            if (!m_Initializes.TryGetValue(id, out DefInfo info) || info.Def == null)
            {
                Type defType = typeof(Def<>).MakeGenericType(configType);
                info.Def = Activator.CreateInstance(defType, info.Handle);
                info.Initialize = targetType.GetConstructor(new Type[] { typeof(Def<>).MakeGenericType(configType) });
                m_Initializes[id] = info;
            }

            return info.Initialize == null
                ? throw new MissingMethodException($"{targetType} constructor with parameter IntPtr not found!")
                : info.Initialize.Invoke(new object[] { info.Def });
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
            public BlobAssetReference<GCHandle> Handle;
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

        public static void AddComponentIData(this EntityCommandBuffer manager, Entity entity, ref object componentData)
        {
            Type DefType = componentData.GetType();

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.CmbBuffAdd == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();
                value.CmbBuffAdd = type.GetMethods()
                    .First(m => m.Name == "AddComponent" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(Entity));

                value.CmbBuffAdd = value.CmbBuffAdd.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.CmbBuffAdd.Invoke(manager, new object[] { entity, componentData });
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

        public static void RemoveComponentIData(this EntityCommandBuffer manager, Entity entity, Type typeComponent)
        {
            Type DefType = typeComponent;

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.CmbBuffDel == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();

                value.CmbBuffDel = type.GetMethods()
                    .First(m => m.Name == "RemoveComponent" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Entity));

                value.CmbBuffDel = value.CmbBuffDel.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.CmbBuffDel.Invoke(manager, new object[] { entity });
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
            public MethodInfo CmbBuffAdd;
            public MethodInfo BakerAdd;

            public MethodInfo ManagerDel;
            public MethodInfo WriterDel;
            public MethodInfo CmbBuffDel;
            public MethodInfo BakerDel;
        }
        private static Dictionary<Type, DefineableInfo> m_Infos = new Dictionary<Type, DefineableInfo>();
    }
}