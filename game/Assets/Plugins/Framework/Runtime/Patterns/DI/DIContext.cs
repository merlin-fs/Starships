using System;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

namespace Common.Core
{
    public interface IDiContext
    {
        T Get<T>(object id = null);
        bool TryGet<T>(out T value, object id = null);
    }

    public abstract partial class DiContext : MonoBehaviour, IDiContext, ISerializationCallbackReceiver
    {
        private static DiContext Current { get; set; }

        private static readonly HashSet<IDiContext> m_Containers = new HashSet<IDiContext>();

        private readonly DIContextContainer m_Container = new DIContextContainer();

        private static DiContext m_Init;
        
        #region IDIContext
        public T Get<T>(object id)
        {
            if (m_Container.TryGet<T>(out T value, id))
                return value;

            foreach (var iter in m_Containers)
            {
                if (iter == (IDiContext)this)
                    continue;

                if (iter.TryGet<T>(out value, id))
                    return value;
            }
            return default;
        }

        public bool TryGet<T>(out T value, object id)
        {
            return m_Container.TryGet<T>(out value, id);
        }
        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            m_Init.Build();
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize() {}
        void ISerializationCallbackReceiver.OnAfterDeserialize() => m_Init = this;
        
        #region MonoBehaviour
        protected virtual void Awake()
        {
            InitializeBinds();
            if (Current == this) return;
            Build();
        }

        private void Build()
        {
            Push(this);
            OnBind();
            InitVars();
        }

        private void InitializeBinds()
        {
            foreach (var iter in GetComponents<IInjectionInitable>())
            {
                iter.Init(this);
            }
            foreach (var iter in m_Container.Objects)
            {
                if (iter is IInjectionInitable init)
                    init.Init(this);
            }
        }
        
        protected virtual void OnDestroy()
        {
            Pop(this);
        }
        #endregion
        protected abstract void OnBind();

        private void Push(IDiContext context)
        {
            m_Containers.Add(context);
            Current = context as DiContext;
        }

        private void Pop(IDiContext context)
        { 
            m_Containers.Remove(context);
            Current = m_Containers.LastOrDefault() as DiContext;
        }

        protected void Bind<T>(T instance, object id = null)
        {
            m_Container.Bind<T>(instance, id);
        }

        private partial class DIContextContainer : IDIContextContainer
        {
            private readonly Dictionary<ContainerType, object> m_Instances = new Dictionary<ContainerType, object>();

            #region IDIContextContainer
            public void Bind<T>(object instance, object id)
            {
                ContainerType t = new ContainerType(typeof(T), id);
                if (m_Instances.ContainsKey(t))
                    Debug.LogError("DIContextContainer.Bind: instance already exists");
                m_Instances[t] = instance;
            }

            public void UnBind<T>(object instance, object id)
            {
                ContainerType t = new ContainerType(typeof(T), id);
                m_Instances.Remove(t);
            }

            public void UnBindAll() => m_Instances.Clear();

            public bool TryGet<T>(out T value, object id)
            {
                value = default;
                var result = TryGet(typeof(T), out object val, id);
                if (result)
                    value = (T)val;
                return result;
            }

            public bool TryGet(Type type, out object value, object id)
            {
                ContainerType t = new ContainerType(type, id);
                return m_Instances.TryGetValue(t, out value);
            }
            #endregion

            public IEnumerable<object> Objects => m_Instances.Values;
            public IEnumerable<Type> Instances => m_Instances.Keys.Select(x => x.Obj);

            struct ContainerType
            {
                public ContainerType(Type obj, object id = null)
                {
                    Obj = obj;
                    Id = id;
                }

                public Type Obj { get; private set; }

                public object Id { get; private set; }

                public override int GetHashCode() => Obj.GetHashCode();

                public override bool Equals(object obj)
                {
                    if (obj == null || GetType() != obj.GetType())
                        return false;

                    ContainerType other = (ContainerType)obj;
                    return other.Obj == this.Obj && other.Id == this.Id;
                }
            }
        }
    }
}