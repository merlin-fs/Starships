using System;
using Game.Core.Events;
using Unity.Entities;

namespace Buildings
{
    public interface IPlaceHolder
    {
        void Cancel();
    }

    public class EventPlace: EventBase<EventPlace>
    {
        public eState State { get; private set; }
        public Entity Entity { get; private set; }

        public enum eState
        {
            New,
            Apply,
            Cancel,
        }

        public EventPlace() { }

        public static EventPlace GetPooled(Entity entity, eState state)
        {
            var e = EventBase<EventPlace>.GetPooled();
            e.Entity = entity;
            e.State = state;
            return e;
        }
    }
}