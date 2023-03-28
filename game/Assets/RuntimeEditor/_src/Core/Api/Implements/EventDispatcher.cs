using System.Collections.Generic;
using UnityEngine.Pool;
using JetBrains.Annotations;
using System;

namespace Game.Core.Events
{
    internal interface IEventDispatchingStrategy
    {
        bool CanDispatchEvent(EventBase evt);

        void DispatchEvent(EventBase evt, IKernel kernel);
    }

    internal enum DispatchMode
    {
        Default = 1,
        Queued = 1,
        Immediate = 2
    }

    //
    // Summary:
    //     Dispatches events to a IPanel.
    public sealed class EventDispatcher
    {
        private struct EventRecord
        {
            public EventBase Event;
            public IKernel Kernel;
            //public StackTrace m_StackTrace;
            //public string stackTrace => m_StackTrace?.ToString() ?? string.Empty;
        }

        private struct DispatchContext
        {
            public uint m_GateCount;

            public Queue<EventRecord> m_Queue;
        }

        //internal ClickDetector m_ClickDetector = new ClickDetector();

        private List<IEventDispatchingStrategy> m_DispatchingStrategies;

        private static readonly ObjectPool<Queue<EventRecord>> k_EventQueuePool = new ObjectPool<Queue<EventRecord>>(() => new Queue<EventRecord>());

        private Queue<EventRecord> m_Queue;

        private uint m_GateCount;

        private Stack<DispatchContext> m_DispatchContexts = new Stack<DispatchContext>();

        private static readonly IEventDispatchingStrategy[] s_EditorStrategies = new IEventDispatchingStrategy[]
        {
            new DefaultDispatchingStrategy()
        };

        private bool m_Immediate = false;

        private bool dispatchImmediately => m_Immediate || m_GateCount == 0;

        internal bool processingEvents { get; private set; }

        public static EventDispatcher CreateDefault()
        {
            return new EventDispatcher(s_EditorStrategies);
        }

        internal static EventDispatcher CreateForRuntime(IList<IEventDispatchingStrategy> strategies)
        {
            return new EventDispatcher(strategies);
        }

        private EventDispatcher(IList<IEventDispatchingStrategy> strategies)
        {
            m_DispatchingStrategies = new List<IEventDispatchingStrategy>();
            m_DispatchingStrategies.AddRange(strategies);
            m_Queue = k_EventQueuePool.Get();
        }

        internal void Dispatch(EventBase evt, [NotNull] IKernel kernel, DispatchMode dispatchMode)
        {
            //evt.MarkReceivedByDispatcher();
            if (dispatchImmediately || dispatchMode == DispatchMode.Immediate)
            {
                ProcessEvent(evt, kernel);
                return;
            }

            evt.Acquire();
            Queue<EventRecord> queue = m_Queue;
            EventRecord item = new EventRecord
            {
                Event = evt,
                Kernel = kernel
            };
            queue.Enqueue(item);
        }

        internal void PushDispatcherContext()
        {
            ProcessEventQueue();
            m_DispatchContexts.Push(new DispatchContext
            {
                m_GateCount = m_GateCount,
                m_Queue = m_Queue
            });
            m_GateCount = 0u;
            m_Queue = k_EventQueuePool.Get();
        }

        internal void PopDispatcherContext()
        {
            UnityEngine.Debug.Assert(m_GateCount == 0, "All gates should have been opened before popping dispatch context.");
            UnityEngine.Debug.Assert(m_Queue.Count == 0, "Queue should be empty when popping dispatch context.");
            k_EventQueuePool.Release(m_Queue);
            m_GateCount = m_DispatchContexts.Peek().m_GateCount;
            m_Queue = m_DispatchContexts.Peek().m_Queue;
            m_DispatchContexts.Pop();
        }

        internal void CloseGate()
        {
            m_GateCount++;
        }

        internal void OpenGate()
        {
            UnityEngine.Debug.Assert(m_GateCount != 0);
            if (m_GateCount != 0)
            {
                m_GateCount--;
            }

            if (m_GateCount == 0)
            {
                ProcessEventQueue();
            }
        }

        private void ProcessEventQueue()
        {
            Queue<EventRecord> queue = m_Queue;
            m_Queue = k_EventQueuePool.Get();
            try
            {
                processingEvents = true;
                while (queue.Count > 0)
                {
                    EventRecord eventRecord = queue.Dequeue();
                    EventBase @event = eventRecord.Event;
                    var kernel = eventRecord.Kernel;
                    try
                    {
                        ProcessEvent(@event, kernel);
                    }
                    finally
                    {
                        @event.Dispose();
                    }
                }
            }
            finally
            {
                processingEvents = false;
                k_EventQueuePool.Release(queue);
            }
        }

        private void ProcessEvent(EventBase evt, [NotNull] IKernel kernel)
        {
            //Event imguiEvent = evt.imguiEvent;
            //bool flag = imguiEvent != null && imguiEvent.rawType == EventType.Used;
            using (new EventDispatcherGate(this))
            {
                //evt.PreDispatch(panel);
                //if (!evt.stopDispatch && !evt.isPropagationStopped)
                {
                    ApplyDispatchingStrategies(evt, kernel);
                }
                /*
                PropagationPaths propagationPaths = evt.path;
                VisualElement visualElement = default(VisualElement);
                int num;
                if (propagationPaths == null && evt.bubblesOrTricklesDown)
                {
                    visualElement = evt.leafTarget as VisualElement;
                    num = ((visualElement != null) ? 1 : 0);
                }
                else
                {
                    num = 0;
                }

                if (num != 0)
                {
                    propagationPaths = (evt.path = PropagationPaths.Build(visualElement, evt));
                    EventDebugger.LogPropagationPaths(evt, propagationPaths);
                }

                if (propagationPaths != null)
                {
                    foreach (VisualElement targetElement in propagationPaths.targetElements)
                    {
                        if (targetElement.panel == panel)
                        {
                            evt.target = targetElement;
                            EventDispatchUtilities.ExecuteDefaultAction(evt);
                        }
                    }

                    evt.target = evt.leafTarget;
                }
                else
                {
                    VisualElement visualElement2 = evt.target as VisualElement;
                    if (visualElement2 == null)
                    {
                        visualElement2 = (VisualElement)(evt.target = panel.visualTree);
                    }

                    if (visualElement2.panel == panel)
                    {
                        EventDispatchUtilities.ExecuteDefaultAction(evt);
                    }
                }
                */
                //evt.PostDispatch(panel);
            }
        }

        private void ApplyDispatchingStrategies(EventBase evt, IKernel kernel)
        {
            foreach (IEventDispatchingStrategy dispatchingStrategy in m_DispatchingStrategies)
            {
                if (dispatchingStrategy.CanDispatchEvent(evt))
                {
                    dispatchingStrategy.DispatchEvent(evt, kernel);
                    /*
                    if (evt.stopDispatch || evt.isPropagationStopped)
                    {
                        break;
                    }
                    */
                }
            }
        }
    }

    internal class DefaultDispatchingStrategy : IEventDispatchingStrategy
    {
        public bool CanDispatchEvent(EventBase evt)
        {
            return true;
        }

        public void DispatchEvent(EventBase evt, IKernel kernel)
        {
            kernel.InvokeCallbacks(evt, PropagationPhase.AtTarget);
        }
    }


    //
    // Summary:
    //     Gates control when the dispatcher processes events.
    public struct EventDispatcherGate : IDisposable, IEquatable<EventDispatcherGate>
    {
        private readonly EventDispatcher m_Dispatcher;

        //
        // Summary:
        //     Constructor.
        //
        // Parameters:
        //   d:
        //     The dispatcher controlled by this gate.
        public EventDispatcherGate(EventDispatcher d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            m_Dispatcher = d;
            m_Dispatcher.CloseGate();
        }

        //
        // Summary:
        //     Implementation of IDisposable.Dispose. Opens the gate. If all gates are open,
        //     events in the queue are processed.
        public void Dispose()
        {
            m_Dispatcher.OpenGate();
        }

        public bool Equals(EventDispatcherGate other)
        {
            return object.Equals(m_Dispatcher, other.m_Dispatcher);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj is EventDispatcherGate && Equals((EventDispatcherGate)obj);
        }

        public override int GetHashCode()
        {
            return (m_Dispatcher != null) ? m_Dispatcher.GetHashCode() : 0;
        }

        public static bool operator ==(EventDispatcherGate left, EventDispatcherGate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EventDispatcherGate left, EventDispatcherGate right)
        {
            return !left.Equals(right);
        }
    }
}
