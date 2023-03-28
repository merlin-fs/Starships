using System;

namespace Game.Core.Events
{
    public delegate void EventCallback<in TEventType>(TEventType evt);
    public delegate void EventCallback<in TEventType, in TCallbackArgs>(TEventType evt, TCallbackArgs userArgs);

    public interface IEventHandler
    {
        void RegisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new();
        void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs) where TEventType : EventBase<TEventType>, new();

        void UnregisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new();
        void UnregisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback) where TEventType : EventBase<TEventType>, new();
    }

    public interface IEventSender: IEventHandler
    {
        void SendEvent(EventBase e);
    }
}