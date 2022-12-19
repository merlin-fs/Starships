using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Common.Core;

namespace Common.Defs
{

    /*
    public abstract class ScriptableDef<T> : ScriptableObject, IDef, ISerializationCallbackReceiver
        where T : unmanaged
    {
        [SerializeField]
        protected DefineableType m_DefineableType;

        [SerializeField]
        private T m_Config;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_DefineableType.SetBaseType(() => null);
            m_DefineableType.SetOwnerType(() => GetType());
        }

        private string m_NameID = null;
        public string NameID => GetNameID;
        protected virtual string GetNameID
        {
            get
            {
                if (string.IsNullOrEmpty(m_NameID))
                    UnityMainThread.Context.Send(o => m_NameID = (o as ScriptableObject) ? (o as ScriptableObject).name : null, this);
                return m_NameID;
            }
        }

        #region IDef
        public void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            var data = CreateInstance();
            InitializeDataConvert(ref data, entity, manager, conversionSystem);
            manager.AddComponentIData(entity, data);
        }

        public void AddComponentData(Entity entity, IBaker manager)
        {
            var data = CreateInstance();
            InitializeDataConvert(ref data, entity, manager);
            manager.AddComponentIData(entity, data);
        }

        public void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            var data = CreateInstance();
            InitializeDataRuntime(ref data);
            writer.AddComponentIData(entity, data, sortKey);
        }

        public void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            //writer.RemoveComponent<T>(sortKey, entity);
        }

        protected IDefineable CreateInstance()
        {
            Type targetType = m_DefineableType.Type;
            if (targetType == null)
                targetType = GetTargetType();

            if (targetType == null)
            {
                throw new NullReferenceException("Target type of def " + GetType().Name + " has a null target type");
            }

            //T value = (T)Activator.CreateInstance(targetType);
            var value = InitDefineable(targetType);
            return value;
        }

        public unsafe IDefineable InitDefineable(Type targetType)
        {
            IDefineable value = default;
            
            if (!m_Initializes.TryGetValue(targetType, out DefInfo info))
            {
                info = new DefInfo() 
                { 
                    Initialize = targetType.GetConstructor(new Type[] { typeof(IntPtr) }), 
                    Config = new IntPtr(UnsafeUtility.AddressOf(ref m_Config)),
            };
                m_Initializes.Add(targetType, info);
            }

            if (info.Initialize == null)
                throw new MissingMethodException($"{targetType} constructor with parameter IntPtr not found!");
            
            value = (IDefineable)Activator.CreateInstance(targetType, info.Config);
            return value;
        }

        protected Type GetTargetType()
        {
            Type findType = GetType();
            if (m_Targets.TryGetValue(findType, out Type value))
                return value;
            DefineableAttribute attr = findType.GetCustomAttribute<DefineableAttribute>(true);
            value = attr?.InstanceType;
            if (value != null)
                return value;
            m_Targets.Add(findType, value);
            return value;
        }

        private struct DefInfo
        {
            public ConstructorInfo Initialize;
            public IntPtr Config;
        }

        private static Dictionary<Type, DefInfo> m_Initializes = new Dictionary<Type, DefInfo>();

        protected static Dictionary<Type, Type> m_Targets = new Dictionary<Type, Type>();

        #endregion
        public ScriptableDef()
        {
            m_DefineableType = new DefineableType(() => typeof(T), () => GetType(), GetTargetType());
        }
        #region entity added
        protected virtual void InitializeDataConvert(ref IDefineable value, Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem) { }
        protected virtual void InitializeDataConvert(ref IDefineable value, Entity entity, IBaker manager) { }
        protected virtual void InitializeDataRuntime(ref IDefineable value) { }
        #endregion
    }
    */
    /*
    [Serializable]
    public abstract class ClassDef<T> : ClassDef, IDef<T>, ISerializationCallbackReceiver
        where T : IDefineable, IComponentData
    {
        [SerializeField]
        protected DefineableType m_DefineableType;

        private GCHandle m_SelfLinkHandle;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_DefineableType.SetBaseType(() => typeof(T));
            m_DefineableType.SetOwnerType(() => GetType());
        }
        #region IDef
        protected T CreateInstance()
        {
            Type targetType = m_DefineableType.Type;
            if (targetType == null)
                targetType = GetTargetType();

            if (targetType == null)
            {
                throw new NullReferenceException("Target type of def " + GetType().Name + " has a null target type");
            }

            //T value = (T)Activator.CreateInstance(targetType);
            T value = InitDefineable(targetType);
            return value;
        }

        public T InitDefineable(Type targetType)
        {
            T value = default;
            if (!m_Initializes.TryGetValue(targetType, out DefInfo info))
            {
                var def = targetType.FindInterfaces(
                    (t, o) =>
                    {
                        return (t.GetInterface(nameof(IDefineable)) != null && t.IsGenericType);

                    }, null);

                if (def.Length > 0)
                {
                    Type gType = def[0].GetGenericArguments()[0];
                    info.ReferenceType = typeof(ReferenceObject<>).MakeGenericType(gType);
                    info.Initialize = targetType.GetConstructor(new Type[] { info.ReferenceType });
                }
                else
                    info = new DefInfo() { Initialize = targetType.GetConstructor(new Type[] { }), ReferenceType = null };

                m_Initializes.Add(targetType, info);
            }

            if (info.Initialize == null)
                throw new MissingMethodException($"{targetType} constructor with parameter {info.ReferenceType} not found!");

            value = info.ReferenceType != null
                ? (T)info.Initialize.Invoke(new object[] { Activator.CreateInstance(info.ReferenceType, m_SelfLinkHandle) })
                : (T)info.Initialize.Invoke(new object[] { });
            return value;
        }

        protected Type GetTargetType()
        {
            Type findType = GetType();
            if (m_Targets.TryGetValue(findType, out Type value))
                return value;
            DefineableAttribute attr = findType.GetCustomAttribute<DefineableAttribute>(true);
            value = attr?.InstanceType;
            if (value != null)
                return value;
            m_Targets.Add(findType, value);
            return value;
        }

        private struct DefInfo
        {
            public ConstructorInfo Initialize;
            public Type ReferenceType;
        }

        private static Dictionary<Type, DefInfo> m_Initializes = new Dictionary<Type, DefInfo>();

        protected static Dictionary<Type, Type> m_Targets = new Dictionary<Type, Type>();

        #endregion
        public ClassDef()
        {
            m_SelfLinkHandle = GCHandle.Alloc(this);
            m_DefineableType = new DefineableType(() => typeof(T), () => GetType(), GetTargetType());
        }

        ~ClassDef()
        {
            m_SelfLinkHandle.Free();
        }

        #region entity added
        protected virtual void InitializeDataConvert(ref T value, Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
        }
        protected virtual void InitializeDataConvert(ref T value, Entity entity, IBaker manager)
        {
        }

        protected virtual void InitializeDataRuntime(ref T value)
        {
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            T data = CreateInstance();
            InitializeDataConvert(ref data, entity, manager, conversionSystem);
            manager.AddComponentIData(entity, data);
        }
        protected override void AddComponentData(Entity entity, IBaker manager)
        {
            T data = CreateInstance();
            InitializeDataConvert(ref data, entity, manager);
            manager.AddComponentIData(entity, data);
        }
        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            T data = CreateInstance();
            InitializeDataRuntime(ref data);
            writer.AddComponentIData(entity, data, sortKey);
        }

        protected override void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            writer.RemoveComponent<T>(sortKey, entity);
        }
        #endregion
    }
    */
}