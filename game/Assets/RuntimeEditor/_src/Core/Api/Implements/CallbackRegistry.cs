using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    internal class EventCallbackRegistry
    {
        private static readonly EventCallbackListPool s_ListPool = new EventCallbackListPool();

        private EventCallbackList m_Callbacks;

        private EventCallbackList m_TemporaryCallbacks;

        private int m_IsInvoking;

        private static EventCallbackList GetCallbackList(EventCallbackList initializer = null)
        {
            return s_ListPool.Get(initializer);
        }

        private static void ReleaseCallbackList(EventCallbackList toRelease)
        {
            s_ListPool.Release(toRelease);
        }

        public EventCallbackRegistry()
        {
            m_IsInvoking = 0;
        }

        private EventCallbackList GetCallbackListForWriting()
        {
            if (m_IsInvoking > 0)
            {
                if (m_TemporaryCallbacks == null)
                {
                    if (m_Callbacks != null)
                    {
                        m_TemporaryCallbacks = GetCallbackList(m_Callbacks);
                    }
                    else
                    {
                        m_TemporaryCallbacks = GetCallbackList();
                    }
                }

                return m_TemporaryCallbacks;
            }

            if (m_Callbacks == null)
            {
                m_Callbacks = GetCallbackList();
            }

            return m_Callbacks;
        }

        private EventCallbackList GetCallbackListForReading()
        {
            if (m_TemporaryCallbacks != null)
            {
                return m_TemporaryCallbacks;
            }

            return m_Callbacks;
        }

        private bool ShouldRegisterCallback(long eventTypeId, Delegate callback)
        {
            if ((object)callback == null)
            {
                return false;
            }

            EventCallbackList callbackListForReading = GetCallbackListForReading();
            if (callbackListForReading != null)
            {
                return !callbackListForReading.Contains(eventTypeId, callback);
            }

            return true;
        }

        private bool UnregisterCallback(long eventTypeId, Delegate callback)
        {
            if ((object)callback == null)
            {
                return false;
            }
            EventCallbackList callbackListForWriting = GetCallbackListForWriting();
            return callbackListForWriting.Remove(eventTypeId, callback);
        }

        public void RegisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            if (callback == null)
            {
                throw new ArgumentException("callback parameter is null");
            }

            long eventTypeId = EventBase<TEventType>.TypeId;
            EventCallbackList callbackListForReading = GetCallbackListForReading();
            if (callbackListForReading == null || !callbackListForReading.Contains(eventTypeId, callback))
            {
                callbackListForReading = GetCallbackListForWriting();
                callbackListForReading.Add(new EventCallbackFunctor<TEventType>(callback));
            }
        }

        public void RegisterCallback<TEventType, TCallbackArgs>(EventCallback<TEventType, TCallbackArgs> callback, TCallbackArgs userArgs) where TEventType : EventBase<TEventType>, new()
        {
            if (callback == null)
            {
                throw new ArgumentException("callback parameter is null");
            }

            long eventTypeId = EventBase<TEventType>.TypeId;
            EventCallbackList callbackListForReading = GetCallbackListForReading();
            if (callbackListForReading != null)
            {
                EventCallbackFunctor<TEventType, TCallbackArgs> eventCallbackFunctor = callbackListForReading.Find(eventTypeId, callback) as EventCallbackFunctor<TEventType, TCallbackArgs>;
                if (eventCallbackFunctor != null)
                {
                    eventCallbackFunctor.userArgs = userArgs;
                    return;
                }
            }

            callbackListForReading = GetCallbackListForWriting();
            callbackListForReading.Add(new EventCallbackFunctor<TEventType, TCallbackArgs>(callback, userArgs));
        }

        public bool UnregisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            long eventTypeId = EventBase<TEventType>.TypeId;
            return UnregisterCallback(eventTypeId, callback);
        }

        public bool UnregisterCallback<TEventType, TCallbackArgs>(EventCallback<TEventType, TCallbackArgs> callback) where TEventType : EventBase<TEventType>, new()
        {
            long eventTypeId = EventBase<TEventType>.TypeId;
            return UnregisterCallback(eventTypeId, callback);
        }

        internal bool TryGetUserArgs<TEventType, TCallbackArgs>(EventCallback<TEventType, TCallbackArgs> callback, out TCallbackArgs userArgs) where TEventType : EventBase<TEventType>, new()
        {
            userArgs = default(TCallbackArgs);
            if (callback == null)
            {
                return false;
            }

            EventCallbackList callbackListForReading = GetCallbackListForReading();
            long eventTypeId = EventBase<TEventType>.TypeId;
            EventCallbackFunctor<TEventType, TCallbackArgs> eventCallbackFunctor = callbackListForReading.Find(eventTypeId, callback) as EventCallbackFunctor<TEventType, TCallbackArgs>;
            if (eventCallbackFunctor == null)
            {
                return false;
            }

            userArgs = eventCallbackFunctor.userArgs;
            return true;
        }

        public void InvokeCallbacks(EventBase evt, PropagationPhase propagationPhase)
        {
            if (m_Callbacks == null)
            {
                return;
            }

            
            m_IsInvoking++;

            int num = 0;
            bool flag = (byte)num != 0;
            for (int i = 0; i < m_Callbacks.Count; i++)
            {
                //if (!flag || m_Callbacks[i].invokePolicy == InvokePolicy.IncludeDisabled)
                {
                    m_Callbacks[i].Invoke(evt, propagationPhase);
                }
            }
            m_IsInvoking--;
            
            if (m_IsInvoking == 0 && m_TemporaryCallbacks != null)
            {
                ReleaseCallbackList(m_Callbacks);
                m_Callbacks = GetCallbackList(m_TemporaryCallbacks);
                ReleaseCallbackList(m_TemporaryCallbacks);
                m_TemporaryCallbacks = null;
            }
        }

        public bool HasTrickleDownHandlers()
        {
            return m_Callbacks != null && m_Callbacks.trickleDownCallbackCount > 0;
        }

        public bool HasBubbleHandlers()
        {
            return m_Callbacks != null && m_Callbacks.bubbleUpCallbackCount > 0;
        }

        private class EventCallbackListPool
        {
            private readonly Stack<EventCallbackList> m_Stack = new Stack<EventCallbackList>();

            public EventCallbackList Get(EventCallbackList initializer)
            {
                EventCallbackList eventCallbackList;
                if (m_Stack.Count == 0)
                {
                    eventCallbackList = ((initializer == null) ? new EventCallbackList() : new EventCallbackList(initializer));
                }
                else
                {
                    eventCallbackList = m_Stack.Pop();
                    if (initializer != null)
                    {
                        eventCallbackList.AddRange(initializer);
                    }
                }

                return eventCallbackList;
            }

            public void Release(EventCallbackList element)
            {
                element.Clear();
                m_Stack.Push(element);
            }
        }
    }
}