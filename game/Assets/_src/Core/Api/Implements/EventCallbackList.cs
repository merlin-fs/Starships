using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    internal class EventCallbackList
    {
        private List<EventCallbackFunctorBase> m_List;

        public int trickleDownCallbackCount { get; private set; }

        public int bubbleUpCallbackCount { get; private set; }

        public int Count => m_List.Count;

        public EventCallbackFunctorBase this[int i]
        {
            get {
                return m_List[i];
            }
            set {
                m_List[i] = value;
            }
        }

        public EventCallbackList()
        {
            m_List = new List<EventCallbackFunctorBase>();
            trickleDownCallbackCount = 0;
            bubbleUpCallbackCount = 0;
        }

        public EventCallbackList(EventCallbackList source)
        {
            m_List = new List<EventCallbackFunctorBase>(source.m_List);
            trickleDownCallbackCount = 0;
            bubbleUpCallbackCount = 0;
        }

        public bool Contains(long eventTypeId, Delegate callback)
        {
            return Find(eventTypeId, callback) != null;
        }

        public EventCallbackFunctorBase Find(long eventTypeId, Delegate callback)
        {
            for (int i = 0; i < m_List.Count; i++)
            {
                if (m_List[i].IsEquivalentTo(eventTypeId, callback))
                {
                    return m_List[i];
                }
            }

            return null;
        }

        public bool Remove(long eventTypeId, Delegate callback)
        {
            for (int i = 0; i < m_List.Count; i++)
            {
                if (m_List[i].IsEquivalentTo(eventTypeId, callback))
                {
                    m_List.RemoveAt(i);
                    bubbleUpCallbackCount--;
                    return true;
                }
            }
            return false;
        }

        public void Add(EventCallbackFunctorBase item)
        {
            m_List.Add(item);
            bubbleUpCallbackCount++;
        }

        public void AddRange(EventCallbackList list)
        {
            m_List.AddRange(list.m_List);
            foreach (EventCallbackFunctorBase item in list.m_List)
            {
                bubbleUpCallbackCount++;
            }
        }

        public void Clear()
        {
            m_List.Clear();
            trickleDownCallbackCount = 0;
            bubbleUpCallbackCount = 0;
        }
    }
}
