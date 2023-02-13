using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityEngine.Pool
{
    public class PoolObjects<T, P> : IDisposable where T : class
    {
        internal readonly List<T> m_List;

        private readonly Func<P, Task<T>> m_CreateFunc;

        private readonly Action<T> m_ActionOnGet;

        private readonly Action<T> m_ActionOnRelease;

        private readonly Action<T> m_ActionOnDestroy;

        private readonly int m_MaxSize;

        internal bool m_CollectionCheck;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;

        public int CountInactive => m_List.Count;

        public PoolObjects(Func<P, Task<T>> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (createFunc == null)
            {
                throw new ArgumentNullException("createFunc");
            }

            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }

            m_List = new List<T>(defaultCapacity);
            m_CreateFunc = createFunc;
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_CollectionCheck = collectionCheck;
        }

        public async Task<T> Get(P value)
        {
            T val;
            if (m_List.Count == 0)
            {
                val = await m_CreateFunc(value);
                CountAll++;
            }
            else
            {
                int index = m_List.Count - 1;
                val = m_List[index];
                m_List.RemoveAt(index);
            }

            m_ActionOnGet?.Invoke(val);
            return val;
        }

        /*
        public async Task<PooledObject> Get(P value, out T v)
        {
            return new PooledObject(v = await Get(value), this);
        }
        */

        public void Release(T element)
        {
            if (m_CollectionCheck && m_List.Count > 0)
            {
                for (int i = 0; i < m_List.Count; i++)
                {
                    if (element == m_List[i])
                    {
                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                    }
                }
            }

            m_ActionOnRelease?.Invoke(element);
            if (CountInactive < m_MaxSize)
            {
                m_List.Add(element);
            }
            else
            {
                m_ActionOnDestroy?.Invoke(element);
            }
        }

        public void Clear()
        {
            if (m_ActionOnDestroy != null)
            {
                foreach (T item in m_List)
                {
                    m_ActionOnDestroy(item);
                }
            }

            m_List.Clear();
            CountAll = 0;
        }

        public void Dispose()
        {
            Clear();
        }

        public struct PooledObject : IDisposable
        {
            private readonly T m_ToReturn;

            private readonly PoolObjects<T, P> m_Pool;

            internal PooledObject(T value, PoolObjects<T, P> pool)
            {
                m_ToReturn = value;
                m_Pool = pool;
            }

            void IDisposable.Dispose()
            {
                m_Pool.Release(m_ToReturn);
            }
        }
    }
}