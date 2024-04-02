using System.Threading;
using Game.Core.Events;

namespace Game.Core
{
    public interface IKernel
    {
        void SendEvent(EventBase e);
        void InvokeCallbacks(EventBase evt, PropagationPhase propagationPhase);
    }

    public abstract class Kernel : IKernel
    {
        public IEventHandler Events { get; }

        private readonly EventDispatcher m_Dispatcher = EventDispatcher.CreateDefault();

        protected Kernel()
        {
            Events = new ProxyEvents(this);
        }

        protected void SendEvent(EventBase e)
        {
            m_Dispatcher.Dispatch(e, this, DispatchMode.Default);
        }

        void IKernel.SendEvent(EventBase e) => SendEvent(e);

        void IKernel.InvokeCallbacks(EventBase evt, PropagationPhase propagationPhase)
        {
            UnityMainThread.Context.Post((obj) =>
            {
                ((ProxyEvents)Events).InvokeCallbacks(evt, propagationPhase);
            }, null);
        }
    }
}