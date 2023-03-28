using System;
using System.Threading.Tasks;
using Common.Defs;
using Game.Core;
using Game.Core.Events;

namespace Game.Core
{
    public interface IKernel
    {
        void SendEvent(EventBase e);
        void InvokeCallbacks(EventBase evt, PropagationPhase propagationPhase);
    }
}

namespace Buildings
{
    public interface IApiEditor
    {
        IEventHandler Events { get; }
        Task<IPlaceHolder> AddEnvironment(IConfig config);
    }
}