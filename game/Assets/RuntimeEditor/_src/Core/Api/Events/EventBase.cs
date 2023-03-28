using System;
using UnityEngine.Pool;

namespace Game.Core.Events
{
    public abstract class EventBase : IDisposable
    {
        internal abstract void Acquire();
        public abstract void Dispose();
        public virtual long eventTypeId => -1L;
    }

    public abstract class EventBase<T> : EventBase where T : EventBase<T>, new()
    {
        private static readonly ObjectPool<T> s_Pool = new ObjectPool<T>(() => new T());

        private int m_RefCount;

        public override long eventTypeId => TypeId;

        protected EventBase()
        {
            m_RefCount = 0;
        }

        public static long TypeId => typeof(T).GetHashCode();

        protected void Init()
        {
            if (m_RefCount != 0)
                m_RefCount = 0;
        }

        public static T GetPooled()
        {
            T val = s_Pool.Get();
            val.Init();
            //val.pooled = true;
            val.Acquire();
            return val;
        }

        internal static T GetPooled(EventBase e)
        {
            T val = GetPooled();
            if (e != null)
            {
                //val.SetTriggerEventId(e.eventId);
            }
            return val;
        }

        private static void ReleasePooled(T evt)
        {
            //if (evt.pooled)
            {
                evt.Init();
                s_Pool.Release(evt);
                //evt.pooled = false;
            }
        }

        internal override void Acquire()
        {
            m_RefCount++;
        }

        public sealed override void Dispose()
        {
            if (--m_RefCount == 0)
            {
                ReleasePooled((T)this);
            }
        }
    }
}