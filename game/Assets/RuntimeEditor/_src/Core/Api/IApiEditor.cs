using System;
using Common.Defs;
using Game.Core.Events;
using Game.Model;

using Unity.Entities;

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
        void AddEnvironment(IConfig config);
        bool TryGetPlaceHolder(Entity entity, out IPlaceHolder holder);
    }

    public interface IApiEditorHandler
    {
        void OnSpawn(Entity entity);
        void OnPlace(Entity entity);
        void OnDestroy(Entity entity);
    }
}