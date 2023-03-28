using System;

namespace Game.Core.Events
{
    //
    // Summary:
    //     The propagation phases of an event.
    public enum PropagationPhase
    {
        //
        // Summary:
        //     The event is not propagated.
        None = 0,
        //
        // Summary:
        //     The event is sent from the panel's root element to the target element's parent.
        TrickleDown = 1,
        //
        // Summary:
        //     The event is sent to the target.
        AtTarget = 2,
        //
        // Summary:
        //     The event is sent to the target element, which can then execute its default actions
        //     for the event at the target phase. Event handlers do not receive the event in
        //     this phase. Instead, ExecuteDefaultActionAtTarget is called on the target element.
        DefaultActionAtTarget = 5,
        //
        // Summary:
        //     The event is sent from the target element's parent back to the panel's root element.
        BubbleUp = 3,
        //
        // Summary:
        //     The event is sent to the target element, which can then execute its final default
        //     actions for the event. Event handlers do not receive the event in this phase.
        //     Instead, ExecuteDefaultAction is called on the target element.
        DefaultAction = 4
    }

    internal abstract class EventCallbackFunctorBase
    {
        protected EventCallbackFunctorBase()
        {
        }

        public abstract void Invoke(EventBase evt, PropagationPhase propagationPhase);

        public abstract bool IsEquivalentTo(long eventTypeId, Delegate callback);

        protected bool PhaseMatches(PropagationPhase propagationPhase)
        {
            if (propagationPhase != PropagationPhase.AtTarget && propagationPhase != PropagationPhase.BubbleUp)
            {
                return false;
            }
            return true;
        }
    }

    internal class EventCallbackFunctor<TEventType> : EventCallbackFunctorBase where TEventType : EventBase<TEventType>, new()
    {
        private readonly EventCallback<TEventType> m_Callback;

        private readonly long m_EventTypeId;

        public EventCallbackFunctor(EventCallback<TEventType> callback)
            : base()
        {
            m_Callback = callback;
            m_EventTypeId = EventBase<TEventType>.TypeId;
        }

        public override void Invoke(EventBase evt, PropagationPhase propagationPhase)
        {
            if (evt == null)
            {
                throw new ArgumentNullException("evt");
            }

            if (evt.eventTypeId == m_EventTypeId && PhaseMatches(propagationPhase))
            {
                //using (new EventDebuggerLogCall(m_Callback, evt))
                {
                    m_Callback(evt as TEventType);
                }
            }
        }

        public override bool IsEquivalentTo(long eventTypeId, Delegate callback)
        {
            return m_EventTypeId == eventTypeId && (Delegate)m_Callback == callback;
        }
    }

    internal class EventCallbackFunctor<TEventType, TCallbackArgs> : EventCallbackFunctorBase where TEventType : EventBase<TEventType>, new()
    {
        private readonly EventCallback<TEventType, TCallbackArgs> m_Callback;

        private readonly long m_EventTypeId;

        internal TCallbackArgs userArgs { get; set; }

        public EventCallbackFunctor(EventCallback<TEventType, TCallbackArgs> callback, TCallbackArgs userArgs)
            : base()
        {
            this.userArgs = userArgs;
            m_Callback = callback;
            m_EventTypeId = EventBase<TEventType>.TypeId;
        }

        public override void Invoke(EventBase evt, PropagationPhase propagationPhase)
        {
            if (evt == null)
            {
                throw new ArgumentNullException("evt");
            }

            if (evt.eventTypeId == m_EventTypeId && PhaseMatches(propagationPhase))
            {
                /* !!!
                using (new EventDebuggerLogCall(m_Callback, evt))
                {
                    m_Callback(evt as TEventType, userArgs);
                }
                */
            }
        }

        public override bool IsEquivalentTo(long eventTypeId, Delegate callback)
        {
            return m_EventTypeId == eventTypeId && (Delegate)m_Callback == callback;
        }
    }
}
