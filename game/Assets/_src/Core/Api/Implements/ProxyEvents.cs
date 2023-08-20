using System;

namespace Game.Core.Events
{
    public class ProxyEvents : IEventHandler, IEventSender
    {
        private EventCallbackRegistry m_EventCallbackRegistry;

        private readonly IKernel m_Kernel;

        public ProxyEvents(IKernel kernel)
        {
            m_Kernel = kernel;
        }

        void IEventSender.SendEvent(EventBase e)
        {
            m_Kernel.SendEvent(e);
        }

        public void InvokeCallbacks(EventBase evt, PropagationPhase propagationPhase)
        {
            EventCallbackRegistry.InvokeCallbacks(evt, propagationPhase);
        }

        private EventCallbackRegistry EventCallbackRegistry
        {
            get 
            { 
                m_EventCallbackRegistry ??= new EventCallbackRegistry();
                return m_EventCallbackRegistry; 
            }
        }

        public void RegisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            EventCallbackRegistry.RegisterCallback(callback);
        }

        public void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs) where TEventType : EventBase<TEventType>, new()
        {
            EventCallbackRegistry.RegisterCallback(callback, userArgs);
        }

        public void UnregisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            EventCallbackRegistry.UnregisterCallback(callback);
        }

        public void UnregisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback) where TEventType : EventBase<TEventType>, new()
        {
            EventCallbackRegistry.UnregisterCallback(callback);
        }
    }
}